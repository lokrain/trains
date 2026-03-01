namespace OpenTTD.Core.Rails
{
    public partial struct TileCenterRailGraph
    {
        /// <summary>
        /// Validates core graph invariants across dense segments, adjacency links, and spatial index.
        /// </summary>
        public bool ValidateInvariants(out string diagnostic)
        {
            for (int i = 0; i < _segs.Count; i++)
            {
                SegmentId segId = new(_segs.DenseId(i));
                NodeId a = _segs.DenseA(i);
                NodeId b = _segs.DenseB(i);

                if (!_nodes.TryGetPos(a, out ushort ax, out ushort ay))
                {
                    diagnostic = $"Missing node position for segment {segId} endpoint A.";
                    return false;
                }

                if (!_nodes.TryGetPos(b, out ushort bx, out ushort by))
                {
                    diagnostic = $"Missing node position for segment {segId} endpoint B.";
                    return false;
                }

                if (!TileEdgeKey.TryFromUnitSegment(ax, ay, bx, by, out uint key))
                {
                    diagnostic = $"Invalid tile-edge mapping for segment {segId}.";
                    return false;
                }

                if (!_spatial.TryGet(key, out SegmentId spatialSeg) || spatialSeg != segId)
                {
                    diagnostic = $"Spatial index mismatch for segment {segId}.";
                    return false;
                }

                if (!ContainsAdjacency(a, segId))
                {
                    diagnostic = $"Missing adjacency link for segment {segId} at node A.";
                    return false;
                }

                if (!ContainsAdjacency(b, segId))
                {
                    diagnostic = $"Missing adjacency link for segment {segId} at node B.";
                    return false;
                }
            }

            diagnostic = string.Empty;
            return true;
        }

        private readonly bool ContainsAdjacency(NodeId node, SegmentId segId)
        {
            int cur = _nodes.GetHeadEdge(node);
            while (cur != -1)
            {
                AdjacencyPool.EdgeRec rec = _adj.Pool[cur];
                if (rec.SegmentId == segId)
                {
                    return true;
                }

                cur = rec.Next;
            }

            return false;
        }
    }
}
