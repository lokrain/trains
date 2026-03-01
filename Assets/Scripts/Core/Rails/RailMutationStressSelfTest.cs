using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Property-style mutation stress self-test for rail graph invariants.
    /// </summary>
    public static class RailMutationStressSelfTest
    {
        public static bool Run()
        {
            TileCenterRailGraph graph = TileCenterRailGraph.Create(Allocator.Temp, 2048, 2048, 8192);
            var active = new NativeList<SegmentId>(Allocator.Temp);
            var events = new NativeList<RailMutationEvent>(Allocator.Temp);
            var deltas = new NativeList<RailSegmentDelta>(Allocator.Temp);

            try
            {
                graph.EnableMutationGuard(true);

                uint rng = 0xC0FFEEu;
                const int ops = 10000;

                for (int i = 0; i < ops; i++)
                {
                    graph.BeginMutation();

                    uint op = Next(ref rng) % 3u;
                    if (op == 0u || active.Length == 0)
                    {
                        ushort ax = (ushort)(1 + (Next(ref rng) % 512));
                        ushort ay = (ushort)(1 + (Next(ref rng) % 512));
                        bool horizontal = (Next(ref rng) & 1u) == 0u;

                        ushort bx = horizontal ? (ushort)(ax + 1) : ax;
                        ushort by = horizontal ? ay : (ushort)(ay + 1);

                        if (graph.TryAddUnitStraight(ax, ay, bx, by, SegmentKind.Straight, SegmentFlags.None, (ushort)(1 + (Next(ref rng) % 8)), out SegmentId id, out _))
                        {
                            active.Add(id);
                        }
                    }
                    else if (op == 1u)
                    {
                        int idx = (int)(Next(ref rng) % (uint)active.Length);
                        SegmentId id = active[idx];
                        if (graph.TryRemove(id))
                        {
                            int last = active.Length - 1;
                            active[idx] = active[last];
                            active.RemoveAt(last);
                        }
                    }
                    else
                    {
                        int idx = (int)(Next(ref rng) % (uint)active.Length);
                        SegmentId id = active[idx];
                        _ = graph.TryUpdateSegmentMetadata(id, SegmentFlags.None, (ushort)(1 + (Next(ref rng) % 16)));
                    }

                    graph.EndMutation();

                    if ((i & 127) == 0)
                    {
                        if (!graph.ValidateInvariants(out _))
                        {
                            return false;
                        }

                        graph.DrainMutationEvents(ref events);
                        RailSegmentDeltaEncoder.Encode(ref events, ref deltas);
                        events.Clear();
                        deltas.Clear();
                    }
                }

                return graph.ValidateInvariants(out _);
            }
            finally
            {
                active.Dispose();
                events.Dispose();
                deltas.Dispose();
                graph.Dispose();
            }
        }

        private static uint Next(ref uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }
    }
}
