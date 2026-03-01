using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Minimal traversal/query self-test for rail graph neighbor and reachability APIs.
    /// </summary>
    public static class RailTraversalSelfTest
    {
        public static bool Run()
        {
            TileCenterRailGraph graph = TileCenterRailGraph.Create(Allocator.Temp, 64, 64, 128);
            try
            {
                graph.EnableMutationGuard(true);
                graph.BeginMutation();

                bool a = graph.TryAddUnitStraight(10, 10, 11, 10, SegmentKind.Straight, SegmentFlags.None, 1, out SegmentId s0, out _);
                bool b = graph.TryAddUnitStraight(11, 10, 12, 10, SegmentKind.Straight, SegmentFlags.None, 1, out SegmentId s1, out _);
                bool c = graph.TryAddUnitStraight(12, 10, 13, 10, SegmentKind.Straight, SegmentFlags.None, 1, out SegmentId s2, out _);

                graph.EndMutation();

                if (!a || !b || !c || !s0.IsValid || !s1.IsValid || !s2.IsValid)
                {
                    return false;
                }

                if (!graph.TryGetSegmentEndpoints(s1, out NodeId midA, out NodeId midB, out _, out _, out _))
                {
                    return false;
                }

                var neighbors = new NativeList<SegmentId>(Allocator.Temp);
                try
                {
                    if (!graph.TryCollectNeighborSegments(midA, ref neighbors))
                    {
                        return false;
                    }

                    bool sawAny = false;
                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        if (neighbors[i] == s0 || neighbors[i] == s1)
                        {
                            sawAny = true;
                        }
                    }

                    if (!sawAny)
                    {
                        return false;
                    }
                }
                finally
                {
                    neighbors.Dispose();
                }

                bool reachableForward = graph.IsReachable(midA, midB, maxDepth: 2, allocator: Allocator.Temp);
                bool reachableFar = graph.IsReachable(midA, new NodeId(99999), maxDepth: 5, allocator: Allocator.Temp);
                bool reachableTightDepth = graph.IsReachable(midA, midB, maxDepth: 0, allocator: Allocator.Temp);

                return reachableForward && !reachableFar && !reachableTightDepth;
            }
            finally
            {
                graph.Dispose();
            }
        }
    }
}
