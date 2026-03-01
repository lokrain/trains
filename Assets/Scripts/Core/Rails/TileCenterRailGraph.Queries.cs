using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    public partial struct TileCenterRailGraph
    {
        /// <summary>
        /// Tries to resolve segment endpoints and metadata by stable segment id.
        /// </summary>
        public bool TryGetSegmentEndpoints(
            SegmentId segmentId,
            out NodeId a,
            out NodeId b,
            out SegmentKind kind,
            out SegmentFlags flags,
            out ushort speedClass)
        {
            if (!_segs.TryGetDenseIndex(segmentId, out int denseIndex))
            {
                a = NodeId.Null;
                b = NodeId.Null;
                kind = SegmentKind.Straight;
                flags = SegmentFlags.None;
                speedClass = 0;
                return false;
            }

            a = _segs.DenseA(denseIndex);
            b = _segs.DenseB(denseIndex);
            kind = _segs.DenseKind(denseIndex);
            flags = _segs.DenseFlags(denseIndex);
            speedClass = _segs.DenseSpeed(denseIndex);
            return true;
        }

        /// <summary>
        /// Collects neighboring segments connected to a node.
        /// </summary>
        /// <param name="node">Node to inspect.</param>
        /// <param name="neighbors">Destination list (appended).</param>
        /// <returns>True when node exists; false if node is invalid/missing.</returns>
        public bool TryCollectNeighborSegments(NodeId node, ref NativeList<SegmentId> neighbors)
        {
            if (!_nodes.TryGetPos(node, out _, out _))
            {
                return false;
            }

            int cur = _nodes.GetHeadEdge(node);
            while (cur != -1)
            {
                AdjacencyPool.EdgeRec rec = _adj.Pool[cur];
                if (rec.SegmentId.IsValid)
                {
                    neighbors.Add(rec.SegmentId);
                }

                cur = rec.Next;
            }

            return true;
        }

        /// <summary>
        /// Path-sanity helper: determines whether goal node is reachable from start node within maxDepth hops.
        /// </summary>
        public bool IsReachable(NodeId start, NodeId goal, int maxDepth, Allocator allocator)
        {
            if (start == goal && start.IsValid)
            {
                return true;
            }

            if (maxDepth <= 0)
            {
                return false;
            }

            if (!_nodes.TryGetPos(start, out _, out _) || !_nodes.TryGetPos(goal, out _, out _))
            {
                return false;
            }

            var frontier = new NativeList<NodeDepth>(allocator);
            var visited = new NativeParallelHashSet<uint>(64, allocator);

            try
            {
                frontier.Add(new NodeDepth { Node = start, Depth = 0 });
                visited.Add(start.Value);

                for (int i = 0; i < frontier.Length; i++)
                {
                    NodeDepth current = frontier[i];
                    if (current.Depth >= maxDepth)
                    {
                        continue;
                    }

                    int edge = _nodes.GetHeadEdge(current.Node);
                    while (edge != -1)
                    {
                        AdjacencyPool.EdgeRec rec = _adj.Pool[edge];
                        if (rec.SegmentId.IsValid && _segs.TryGetDenseIndex(rec.SegmentId, out int denseIndex))
                        {
                            NodeId a = _segs.DenseA(denseIndex);
                            NodeId b = _segs.DenseB(denseIndex);
                            NodeId next = a == current.Node ? b : a;

                            if (next == goal)
                            {
                                return true;
                            }

                            if (next.IsValid && visited.Add(next.Value))
                            {
                                frontier.Add(new NodeDepth { Node = next, Depth = current.Depth + 1 });
                            }
                        }

                        edge = rec.Next;
                    }
                }

                return false;
            }
            finally
            {
                frontier.Dispose();
                visited.Dispose();
            }
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

        private struct NodeDepth
        {
            public NodeId Node;
            public int Depth;
        }
    }
}
