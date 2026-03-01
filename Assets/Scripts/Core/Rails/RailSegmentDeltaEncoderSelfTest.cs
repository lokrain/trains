using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Self-test for mutation-event to delta encoding flow.
    /// </summary>
    public static class RailSegmentDeltaEncoderSelfTest
    {
        public static bool Run()
        {
            TileCenterRailGraph graph = TileCenterRailGraph.Create(Allocator.Temp, 64, 64, 128);
            try
            {
                graph.EnableMutationGuard(true);
                graph.BeginMutation();

                bool created = graph.TryAddUnitStraight(30, 30, 31, 30, SegmentKind.Straight, SegmentFlags.None, 2, out SegmentId id, out _);
                bool updated = graph.TryUpdateSegmentMetadata(id, SegmentFlags.None, 7);
                bool removed = graph.TryRemove(id);

                graph.EndMutation();

                if (!created || !updated || !removed)
                {
                    return false;
                }

                var events = new NativeList<RailMutationEvent>(Allocator.Temp);
                var deltas = new NativeList<RailSegmentDelta>(Allocator.Temp);
                try
                {
                    graph.DrainMutationEvents(ref events);
                    RailSegmentDeltaEncoder.Encode(ref events, ref deltas);

                    if (deltas.Length != 3)
                    {
                        return false;
                    }

                    if (deltas[0].Op != RailMutationOp.Create || deltas[0].SegmentId != id)
                    {
                        return false;
                    }

                    if (deltas[1].Op != RailMutationOp.Update || deltas[1].SegmentSpec.SpeedClass != 7)
                    {
                        return false;
                    }

                    if (deltas[2].Op != RailMutationOp.Remove || deltas[2].SegmentId != id)
                    {
                        return false;
                    }

                    return deltas[0].TopologyVersion < deltas[1].TopologyVersion
                        && deltas[1].TopologyVersion < deltas[2].TopologyVersion;
                }
                finally
                {
                    events.Dispose();
                    deltas.Dispose();
                }
            }
            finally
            {
                graph.Dispose();
            }
        }
    }
}
