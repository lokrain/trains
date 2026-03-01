using System;
using System.Runtime.CompilerServices;

namespace OpenTTD.Core.Rails
{
    public partial struct TileCenterRailGraph
    {
        /// <summary>
        /// Tries to add a unit-length axis-aligned straight segment.
        /// </summary>
        public bool TryAddUnitStraight(
            ushort ax,
            ushort ay,
            ushort bx,
            ushort by,
            SegmentKind kind,
            SegmentFlags flags,
            ushort speedClass,
            out SegmentId segId,
            out uint tileEdgeKey)
        {
            EnsureMutationPhase();

            segId = SegmentId.Null;
            tileEdgeKey = 0;

            if (!TileEdgeKey.TryFromUnitSegment(ax, ay, bx, by, out uint key))
            {
                return false;
            }

            if (_spatial.TryGet(key, out _))
            {
                return false;
            }

            uint packedA = NodeTable.PackPos(ax, ay);
            uint packedB = NodeTable.PackPos(bx, by);
            bool createdNodeA = false;
            bool createdNodeB = false;

            if (!_nodes.TryGetNode(packedA, out NodeId aNode))
            {
                aNode = new NodeId(_nodeIds.Alloc());
                _nodes.InsertNode(aNode, packedA);
                createdNodeA = true;
            }

            if (!_nodes.TryGetNode(packedB, out NodeId bNode))
            {
                bNode = new NodeId(_nodeIds.Alloc());
                _nodes.InsertNode(bNode, packedB);
                createdNodeB = true;
            }

            segId = _segIds.Alloc();
            _segs.Add(segId, aNode, bNode, kind, flags, speedClass, out int denseIndex);

            bool spatialAdded = false;
            bool adjAAdded = false;
            bool adjBAdded = false;
            int headA = -1;
            int headB = -1;
            int allocatedAdjA = -1;
            int allocatedAdjB = -1;

            if (!_spatial.TryAdd(key, segId))
            {
                _segs.RemoveSwap(segId, denseIndex);
                _segIds.Free(segId);

                if (createdNodeA)
                {
                    _nodes.RemoveNodeMapping(aNode);
                    _nodeIds.Free(aNode.Value);
                }

                if (createdNodeB)
                {
                    _nodes.RemoveNodeMapping(bNode);
                    _nodeIds.Free(bNode.Value);
                }

                return false;
            }

            spatialAdded = true;

            headA = _nodes.GetHeadEdge(aNode);
            allocatedAdjA = _adj.Alloc(segId, headA);
            _nodes.SetHeadEdge(aNode, allocatedAdjA);
            adjAAdded = true;

            headB = _nodes.GetHeadEdge(bNode);
            allocatedAdjB = _adj.Alloc(segId, headB);
            _nodes.SetHeadEdge(bNode, allocatedAdjB);
            adjBAdded = true;

            TopologyVersion++;

            try
            {
                _mutationEvents.Add(new RailMutationEvent
                {
                    Op = RailMutationOp.Create,
                    SegmentId = segId,
                    SegmentSpec = GetSegmentSpec16ByDenseIndex(denseIndex),
                    TopologyVersion = TopologyVersion
                });
            }
            catch
            {
                TopologyVersion--;

                if (adjBAdded)
                {
                    _adj.Free(allocatedAdjB);
                    _nodes.SetHeadEdge(bNode, headB);
                }

                if (adjAAdded)
                {
                    _adj.Free(allocatedAdjA);
                    _nodes.SetHeadEdge(aNode, headA);
                }

                if (spatialAdded)
                {
                    _spatial.Remove(key);
                }

                _segs.RemoveSwap(segId, denseIndex);
                _segIds.Free(segId);

                if (createdNodeA)
                {
                    _nodes.RemoveNodeMapping(aNode);
                    _nodeIds.Free(aNode.Value);
                }

                if (createdNodeB)
                {
                    _nodes.RemoveNodeMapping(bNode);
                    _nodeIds.Free(bNode.Value);
                }

                return false;
            }

            tileEdgeKey = key;
            return true;
        }

        /// <summary>
        /// Tries to remove a segment by stable id.
        /// </summary>
        public bool TryRemove(SegmentId segId)
        {
            EnsureMutationPhase();

            if (!_segs.TryGetDenseIndex(segId, out int denseIndex))
            {
                return false;
            }

            NodeId aNode = _segs.DenseA(denseIndex);
            NodeId bNode = _segs.DenseB(denseIndex);

            if (!_nodes.TryGetPos(aNode, out ushort ax, out ushort ay))
            {
                return false;
            }

            if (!_nodes.TryGetPos(bNode, out ushort bx, out ushort by))
            {
                return false;
            }

            if (!TileEdgeKey.TryFromUnitSegment(ax, ay, bx, by, out uint key))
            {
                return false;
            }

            _spatial.Remove(key);

            UnlinkAdj(aNode, segId);
            UnlinkAdj(bNode, segId);

            TryPruneNodeIfIsolated(aNode);
            if (bNode != aNode)
            {
                TryPruneNodeIfIsolated(bNode);
            }

            _segs.RemoveSwap(segId, denseIndex);
            _segIds.Free(segId);

            TopologyVersion++;

            _mutationEvents.Add(new RailMutationEvent
            {
                Op = RailMutationOp.Remove,
                SegmentId = segId,
                SegmentSpec = default,
                TopologyVersion = TopologyVersion
            });

            return true;
        }

        /// <summary>
        /// Tries to update mutable segment metadata and emits an update mutation event.
        /// </summary>
        public bool TryUpdateSegmentMetadata(SegmentId segId, SegmentFlags flags, ushort speedClass)
        {
            EnsureMutationPhase();

            if (!_segs.TryGetDenseIndex(segId, out int denseIndex))
            {
                return false;
            }

            _segs.SetDenseMetadata(denseIndex, flags, speedClass);

            TopologyVersion++;
            _mutationEvents.Add(new RailMutationEvent
            {
                Op = RailMutationOp.Update,
                SegmentId = segId,
                SegmentSpec = GetSegmentSpec16ByDenseIndex(denseIndex),
                TopologyVersion = TopologyVersion
            });

            return true;
        }

        /// <summary>
        /// Compaction hook: rebuild adjacency pool when fragmentation exceeds threshold.
        /// Segment IDs remain stable; only internal storage is rewritten.
        /// </summary>
        public void CompactAdjacency(float fragmentationThreshold = 0.35f)
        {
            EnsureMutationPhase();

            int poolLen = _adj.Pool.Length;
            if (poolLen < 4096)
            {
                return;
            }

            int freeCount = 0;
            for (int i = _adj.FreeHead; i != -1;)
            {
                freeCount++;
                i = _adj.Pool[i].Next;
            }

            float frag = (float)freeCount / poolLen;
            if (frag < fragmentationThreshold)
            {
                return;
            }

            AdjacencyPool newAdj = AdjacencyPool.Create(poolLen - freeCount + 512, _adj.Allocator);

            int maxNodeIndex = _nodes.MaxNodeIndex;
            for (int n = 1; n <= maxNodeIndex; n++)
            {
                NodeId node = new((uint)n);
                int oldCur = _nodes.GetHeadEdge(node);
                int newHead = -1;

                while (oldCur != -1)
                {
                    AdjacencyPool.EdgeRec old = _adj.Pool[oldCur];
                    if (old.SegmentId != SegmentId.Null)
                    {
                        newHead = newAdj.Alloc(old.SegmentId, newHead);
                    }

                    oldCur = old.Next;
                }

                _nodes.SetHeadEdge(node, newHead);
            }

            _adj.Dispose();
            _adj = newAdj;
            TopologyEpoch++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UnlinkAdj(NodeId node, SegmentId segId)
        {
            int head = _nodes.GetHeadEdge(node);
            int prev = -1;
            int cur = head;

            while (cur != -1)
            {
                ref AdjacencyPool.EdgeRec rec = ref _adj.ElementAt(cur);
                if (rec.SegmentId == segId)
                {
                    int next = rec.Next;
                    if (prev == -1)
                    {
                        _nodes.SetHeadEdge(node, next);
                    }
                    else
                    {
                        ref AdjacencyPool.EdgeRec prevRec = ref _adj.ElementAt(prev);
                        prevRec.Next = next;
                    }

                    _adj.Free(cur);
                    return;
                }

                prev = cur;
                cur = rec.Next;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void EnsureMutationPhase()
        {
            if (_enforceMutationPhase != 0 && _mutationPhase == 0)
            {
                throw new InvalidOperationException("Rail graph mutation attempted outside explicit mutation phase.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryPruneNodeIfIsolated(NodeId node)
        {
            if (!node.IsValid)
            {
                return;
            }

            if (_nodes.GetHeadEdge(node) != -1)
            {
                return;
            }

            _nodes.RemoveNodeMapping(node);
            _nodeIds.Free(node.Value);
        }
    }
}
