using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Self-test for deterministic rail mutation event emission and drain behavior.
    /// </summary>
    public static class RailMutationEventsSelfTest
    {
        public static bool Run()
        {
            TileCenterRailGraph graph = TileCenterRailGraph.Create(Allocator.Temp, 32, 32, 64);
            try
            {
                graph.EnableMutationGuard(true);
                graph.BeginMutation();

                bool created = graph.TryAddUnitStraight(20, 20, 21, 20, SegmentKind.Straight, SegmentFlags.None, 2, out SegmentId id, out _);
                bool removed = graph.TryRemove(id);

                graph.EndMutation();

                if (!created || !removed)
                {
                    return false;
                }

                var drained = new NativeList<RailMutationEvent>(Allocator.Temp);
                try
                {
                    graph.DrainMutationEvents(ref drained);
                    if (drained.Length != 2)
                    {
                        return false;
                    }

                    if (drained[0].Op != RailMutationOp.Create || drained[0].SegmentId != id || drained[0].TopologyVersion == 0)
                    {
                        return false;
                    }

                    if (drained[1].Op != RailMutationOp.Remove || drained[1].SegmentId != id)
                    {
                        return false;
                    }

                    var secondDrain = new NativeList<RailMutationEvent>(Allocator.Temp);
                    try
                    {
                        graph.DrainMutationEvents(ref secondDrain);
                        return secondDrain.Length == 0;
                    }
                    finally
                    {
                        secondDrain.Dispose();
                    }
                }
                finally
                {
                    drained.Dispose();
                }
            }
            finally
            {
                graph.Dispose();
            }
        }
    }
}
