using System;
using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Minimal self-test coverage for rail graph core allocator, mutation guard, and invariants.
    /// </summary>
    public static class RailGraphCoreSelfTest
    {
        public static bool Run()
        {
            return SegmentIdAllocatorChurn()
                && MutationGuardBehavior()
                && GraphInvariantValidation()
                && DuplicateAddDoesNotCorruptState();
        }

        private static bool SegmentIdAllocatorChurn()
        {
            SegmentIdAllocator allocator = SegmentIdAllocator.Create(1, Allocator.Temp, 64);
            try
            {
                var active = new NativeParallelHashSet<uint>(128, Allocator.Temp);
                try
                {
                    for (int i = 0; i < 64; i++)
                    {
                        SegmentId id = allocator.Alloc();
                        if (!id.IsValid || !active.Add(id.Value))
                        {
                            return false;
                        }
                    }

                    int removed = 0;
                    foreach (uint id in active)
                    {
                        if ((id & 1u) == 0)
                        {
                            allocator.Free(new SegmentId(id));
                            removed++;
                        }
                    }

                    if (removed == 0)
                    {
                        return false;
                    }

                    for (int i = 0; i < removed; i++)
                    {
                        SegmentId id = allocator.Alloc();
                        if (!id.IsValid)
                        {
                            return false;
                        }
                    }

                    return allocator.ActiveCount > 0;
                }
                finally
                {
                    active.Dispose();
                }
            }
            finally
            {
                allocator.Dispose();
            }
        }

        private static bool MutationGuardBehavior()
        {
            TileCenterRailGraph graph = TileCenterRailGraph.Create(Allocator.Temp, 16, 16, 32);
            try
            {
                graph.EnableMutationGuard(true);

                bool threw = false;
                try
                {
                    _ = graph.TryAddUnitStraight(1, 1, 2, 1, SegmentKind.Straight, SegmentFlags.None, 1, out _, out _);
                }
                catch (InvalidOperationException)
                {
                    threw = true;
                }

                if (!threw)
                {
                    return false;
                }

                graph.BeginMutation();
                bool ok = graph.TryAddUnitStraight(1, 1, 2, 1, SegmentKind.Straight, SegmentFlags.None, 1, out _, out _);
                graph.EndMutation();
                return ok;
            }
            finally
            {
                graph.Dispose();
            }
        }

        private static bool GraphInvariantValidation()
        {
            TileCenterRailGraph graph = TileCenterRailGraph.Create(Allocator.Temp, 32, 32, 64);
            try
            {
                graph.EnableMutationGuard(true);
                graph.BeginMutation();

                bool a = graph.TryAddUnitStraight(10, 10, 11, 10, SegmentKind.Straight, SegmentFlags.None, 3, out SegmentId s0, out _);
                bool b = graph.TryAddUnitStraight(11, 10, 12, 10, SegmentKind.Straight, SegmentFlags.None, 3, out SegmentId s1, out _);
                bool c = graph.TryRemove(s0);

                graph.EndMutation();

                if (!a || !b || !c || !s1.IsValid)
                {
                    return false;
                }

                return graph.ValidateInvariants(out _);
            }
            finally
            {
                graph.Dispose();
            }
        }

        private static bool DuplicateAddDoesNotCorruptState()
        {
            TileCenterRailGraph graph = TileCenterRailGraph.Create(Allocator.Temp, 16, 16, 32);
            try
            {
                graph.EnableMutationGuard(true);
                graph.BeginMutation();

                bool added = graph.TryAddUnitStraight(4, 4, 5, 4, SegmentKind.Straight, SegmentFlags.None, 2, out _, out _);
                int beforeDuplicate = graph.SegmentCount;
                bool duplicateAdded = graph.TryAddUnitStraight(4, 4, 5, 4, SegmentKind.Straight, SegmentFlags.None, 2, out _, out _);

                graph.EndMutation();

                if (!added || duplicateAdded)
                {
                    return false;
                }

                if (graph.SegmentCount != beforeDuplicate)
                {
                    return false;
                }

                return graph.ValidateInvariants(out _);
            }
            finally
            {
                graph.Dispose();
            }
        }
    }
}
