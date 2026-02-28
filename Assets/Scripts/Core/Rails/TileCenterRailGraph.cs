using System;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Orchestrates rail graph storage with stable IDs, node table, adjacency pool,
    /// dense segment storage, and tile-edge spatial index.
    /// </summary>
    public struct TileCenterRailGraph : IDisposable
    {
        /// <summary>
        /// Increments on logical topology mutation (add/remove).
        /// </summary>
        public uint TopologyVersion;

        /// <summary>
        /// Increments on internal compaction passes.
        /// </summary>
        public uint TopologyEpoch;

        private IdAllocator _segIds;
        private IdAllocator _nodeIds;

        private NodeTable _nodes;
        private AdjacencyPool _adj;
        private SegmentStoreSoA _segs;
        private RailSpatialIndex _spatial;

        /// <summary>
        /// Creates a tile-center rail graph.
        /// </summary>
        public static TileCenterRailGraph Create(Allocator alloc, int segCap = 4096, int nodeCap = 4096, int adjCap = 16384)
        {
            return new TileCenterRailGraph
            {
                TopologyVersion = 0,
                TopologyEpoch = 0,
                _segIds = IdAllocator.Create(1, alloc),
                _nodeIds = IdAllocator.Create(1, alloc),
                _nodes = NodeTable.Create(nodeCap, alloc),
                _adj = AdjacencyPool.Create(adjCap, alloc),
                _segs = SegmentStoreSoA.Create(segCap, alloc),
                _spatial = RailSpatialIndex.Create(segCap * 4, alloc)
            };
        }

        /// <summary>
        /// Number of active segments.
        /// </summary>
        public readonly int SegmentCount => _segs.Count;

        public void Dispose()
        {
            _segIds.Dispose();
            _nodeIds.Dispose();
            _nodes.Dispose();
            _adj.Dispose();
            _segs.Dispose();
            _spatial.Dispose();
        }

        /// <summary>
        /// Builds a 16-byte protocol/event segment spec from dense index.
        /// </summary>
        public SegmentSpec16 GetSegmentSpec16ByDenseIndex(int denseIndex)
        {
            NodeId a = _segs.DenseA(denseIndex);
            NodeId b = _segs.DenseB(denseIndex);

            _ = _nodes.TryGetPos(a, out ushort ax, out ushort ay);
            _ = _nodes.TryGetPos(b, out ushort bx, out ushort by);

            return new SegmentSpec16
            {
                Ax = ax,
                Ay = ay,
                Bx = bx,
                By = by,
                Kind = _segs.DenseKind(denseIndex),
                Flags = _segs.DenseFlags(denseIndex),
                SpeedClass = _segs.DenseSpeed(denseIndex),
                SegmentId = new SegmentId(_segs.DenseId(denseIndex))
            };
        }

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

            if (!_nodes.TryGetNode(packedA, out NodeId aNode))
            {
                aNode = new NodeId(_nodeIds.Alloc());
                _nodes.InsertNode(aNode, packedA);
            }

            if (!_nodes.TryGetNode(packedB, out NodeId bNode))
            {
                bNode = new NodeId(_nodeIds.Alloc());
                _nodes.InsertNode(bNode, packedB);
            }

            segId = new SegmentId(_segIds.Alloc());

            _segs.Add(segId, aNode, bNode, kind, flags, speedClass, out _);

            if (!_spatial.TryAdd(key, segId))
            {
                return false;
            }

            int headA = _nodes.GetHeadEdge(aNode);
            _nodes.SetHeadEdge(aNode, _adj.Alloc(segId, headA));

            int headB = _nodes.GetHeadEdge(bNode);
            _nodes.SetHeadEdge(bNode, _adj.Alloc(segId, headB));

            TopologyVersion++;
            tileEdgeKey = key;
            return true;
        }

        /// <summary>
        /// Tries to remove a segment by stable id.
        /// </summary>
        public bool TryRemove(SegmentId segId)
        {
            if (!_segs.TryGetDenseIndex(segId, out int denseIndex))
            {
                return false;
            }

            NodeId aNode = _segs.DenseA(denseIndex);
            NodeId bNode = _segs.DenseB(denseIndex);

            _ = _nodes.TryGetPos(aNode, out ushort ax, out ushort ay);
            _ = _nodes.TryGetPos(bNode, out ushort bx, out ushort by);

            if (!TileEdgeKey.TryFromUnitSegment(ax, ay, bx, by, out uint key))
            {
                return false;
            }

            _spatial.Remove(key);

            UnlinkAdj(aNode, segId);
            UnlinkAdj(bNode, segId);

            _segs.RemoveSwap(segId, denseIndex);
            _segIds.Free(segId.Value);

            TopologyVersion++;
            return true;
        }

        /// <summary>
        /// Compaction hook: rebuild adjacency pool when fragmentation exceeds threshold.
        /// Segment IDs remain stable; only internal storage is rewritten.
        /// </summary>
        public void CompactAdjacency(float fragmentationThreshold = 0.35f)
        {
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

            int maxNodeIndex = GetMaxNodeIndex();
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
        private readonly int GetMaxNodeIndex()
        {
            // Conservative cap for now; can be replaced with explicit NodeTable max index API.
            return 65536;
        }
    }
}
