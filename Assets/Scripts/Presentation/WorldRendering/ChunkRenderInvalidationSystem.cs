#nullable enable
using Unity.Collections;
using Unity.Entities;

namespace OpenTTD.Presentation.WorldRendering
{
    /// <summary>
    /// Applies queued chunk render invalidation events to presenter entities.
    /// Includes a basic version fence by dropping stale invalidations.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(ChunkPresenterLifecycleSystem))]
    public partial class ChunkRenderInvalidationSystem : SystemBase
    {
        private const int ChunksW = 32;
        private const int ChunksH = 32;
        private const int ChunkSize = 64;

        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonEntity<ChunkRenderInvalidationSource>(out Entity sourceEntity))
            {
                return;
            }

            int maxPerFrame = 256;
            if (SystemAPI.TryGetSingleton<ChunkMeshRebuildBudget>(out ChunkMeshRebuildBudget budget)
                && budget.MaxInvalidationsPerFrame > 0)
            {
                maxPerFrame = budget.MaxInvalidationsPerFrame;
            }

            DynamicBuffer<ChunkRenderInvalidationEvent> events = EntityManager.GetBuffer<ChunkRenderInvalidationEvent>(sourceEntity);
            if (events.Length == 0)
            {
                return;
            }

            int processCount = events.Length;
            if (processCount > maxPerFrame)
            {
                processCount = maxPerFrame;
            }

            var presenters = new NativeList<Entity>(Allocator.Temp);
            foreach (var (_, entity) in SystemAPI.Query<RefRO<ChunkRenderTag>>().WithEntityAccess())
            {
                presenters.Add(entity);
            }

            for (int ei = 0; ei < processCount; ei++)
            {
                ChunkRenderInvalidationEvent ev = events[ei];
                if ((uint)ev.ChunkX >= ChunksW || (uint)ev.ChunkY >= ChunksH)
                {
                    continue;
                }

                for (int i = 0; i < presenters.Length; i++)
                {
                    Entity presenter = presenters[i];
                    ChunkCoordComponent coord = EntityManager.GetComponentData<ChunkCoordComponent>(presenter);
                    if (coord.X != ev.ChunkX || coord.Y != ev.ChunkY)
                    {
                        continue;
                    }

                    ChunkRenderPendingVersion pending = EntityManager.GetComponentData<ChunkRenderPendingVersion>(presenter);
                    if (ev.SnapshotVersion < pending.Value)
                    {
                        break;
                    }

                    pending.Value = ev.SnapshotVersion;
                    EntityManager.SetComponentData(presenter, pending);

                    ChunkMeshDirty dirty = EntityManager.GetComponentData<ChunkMeshDirty>(presenter);
                    if (ev.Mode == ChunkMeshDirtyMode.Full)
                    {
                        dirty.Mode = ChunkMeshDirtyMode.Full;
                        dirty.MinX = 0;
                        dirty.MinY = 0;
                        dirty.MaxX = (byte)(ChunkSize - 1);
                        dirty.MaxY = (byte)(ChunkSize - 1);
                    }
                    else if (ev.Mode == ChunkMeshDirtyMode.Rect)
                    {
                        if (dirty.Mode != ChunkMeshDirtyMode.Full)
                        {
                            if (dirty.Mode == ChunkMeshDirtyMode.Rect)
                            {
                                if (ev.MinX < dirty.MinX)
                                {
                                    dirty.MinX = ev.MinX;
                                }

                                if (ev.MinY < dirty.MinY)
                                {
                                    dirty.MinY = ev.MinY;
                                }

                                if (ev.MaxX > dirty.MaxX)
                                {
                                    dirty.MaxX = ev.MaxX;
                                }

                                if (ev.MaxY > dirty.MaxY)
                                {
                                    dirty.MaxY = ev.MaxY;
                                }
                            }
                            else
                            {
                                dirty.Mode = ChunkMeshDirtyMode.Rect;
                                dirty.MinX = ev.MinX;
                                dirty.MinY = ev.MinY;
                                dirty.MaxX = ev.MaxX;
                                dirty.MaxY = ev.MaxY;
                            }
                        }
                    }

                    EntityManager.SetComponentData(presenter, dirty);
                    break;
                }
            }

            if (processCount >= events.Length)
            {
                events.Clear();
            }
            else
            {
                int remaining = events.Length - processCount;
                for (int i = 0; i < remaining; i++)
                {
                    events[i] = events[processCount + i];
                }

                events.ResizeUninitialized(remaining);
            }
            presenters.Dispose();
        }
    }
}
