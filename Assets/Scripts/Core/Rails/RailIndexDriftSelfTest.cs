using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Stress self-test for graph+spatial index consistency under randomized mutation churn.
    /// </summary>
    public static class RailIndexDriftSelfTest
    {
        public static bool Run()
        {
            TileCenterRailGraph graph = TileCenterRailGraph.Create(Allocator.Temp, 4096, 4096, 16384);
            var active = new NativeList<SegmentId>(Allocator.Temp);

            try
            {
                graph.EnableMutationGuard(true);

                uint rng = 0x9E3779B9u;
                const int ops = 20000;

                for (int i = 0; i < ops; i++)
                {
                    graph.BeginMutation();

                    uint choice = Next(ref rng) % 3u;
                    if (choice == 0u || active.Length == 0)
                    {
                        ushort ax = (ushort)(1 + (Next(ref rng) % 1024));
                        ushort ay = (ushort)(1 + (Next(ref rng) % 1024));
                        bool horizontal = (Next(ref rng) & 1u) == 0u;

                        ushort bx = horizontal ? (ushort)(ax + 1) : ax;
                        ushort by = horizontal ? ay : (ushort)(ay + 1);

                        if (graph.TryAddUnitStraight(
                            ax,
                            ay,
                            bx,
                            by,
                            SegmentKind.Straight,
                            SegmentFlags.None,
                            (ushort)(1 + (Next(ref rng) % 12)),
                            out SegmentId id,
                            out _))
                        {
                            active.Add(id);
                        }
                    }
                    else if (choice == 1u)
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
                        _ = graph.TryUpdateSegmentMetadata(id, SegmentFlags.None, (ushort)(1 + (Next(ref rng) % 20)));
                    }

                    graph.EndMutation();

                    if ((i & 255) == 0)
                    {
                        if (!graph.ValidateInvariants(out _))
                        {
                            return false;
                        }
                    }
                }

                return graph.ValidateInvariants(out _);
            }
            finally
            {
                active.Dispose();
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
