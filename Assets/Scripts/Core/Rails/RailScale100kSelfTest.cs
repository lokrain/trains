using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Scalability self-test scaffold for 100k segment class workloads.
    /// </summary>
    public static class RailScale100kSelfTest
    {
        public static bool Run()
        {
            const int targetSegments = 100_000;

            TileCenterRailGraph graph = TileCenterRailGraph.Create(Allocator.Temp, 131072, 131072, 262144);
            var active = new NativeList<SegmentId>(Allocator.Temp);

            try
            {
                graph.EnableMutationGuard(true);

                graph.BeginMutation();
                for (int i = 0; i < targetSegments; i++)
                {
                    ushort ax = (ushort)(1 + (i % 1000));
                    ushort ay = (ushort)(1 + (i / 1000));
                    ushort bx = (ushort)(ax + 1);
                    ushort by = ay;

                    if (graph.TryAddUnitStraight(
                        ax,
                        ay,
                        bx,
                        by,
                        SegmentKind.Straight,
                        SegmentFlags.None,
                        speedClass: 4,
                        out SegmentId id,
                        out _))
                    {
                        active.Add(id);
                    }
                }
                graph.EndMutation();

                if (active.Length < targetSegments)
                {
                    return false;
                }

                if (!graph.ValidateInvariants(out _))
                {
                    return false;
                }

                graph.BeginMutation();
                int removeCount = active.Length / 4;
                for (int i = 0; i < removeCount; i++)
                {
                    SegmentId id = active[i];
                    _ = graph.TryRemove(id);
                }
                graph.EndMutation();

                if (!graph.ValidateInvariants(out _))
                {
                    return false;
                }

                return graph.SegmentCount > 0;
            }
            finally
            {
                active.Dispose();
                graph.Dispose();
            }
        }
    }
}
