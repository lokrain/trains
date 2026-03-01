#nullable enable
using Unity.Entities;

namespace OpenTTD.Presentation.WorldRendering
{
    /// <summary>
    /// Minimal self-test for invalidation-to-dirty wiring and stale-version rejection.
    /// </summary>
    public static class ChunkRenderInvalidationSelfTest
    {
        public static bool Run()
        {
            using var world = new World("ChunkRenderInvalidationSelfTest");
            var entityManager = world.EntityManager;
            var system = world.CreateSystemManaged<ChunkRenderInvalidationSystem>();

            Entity source = entityManager.CreateEntity(typeof(ChunkRenderInvalidationSource));
            DynamicBuffer<ChunkRenderInvalidationEvent> events = entityManager.AddBuffer<ChunkRenderInvalidationEvent>(source);

            Entity presenter = entityManager.CreateEntity(
                typeof(ChunkRenderTag),
                typeof(ChunkCoordComponent),
                typeof(ChunkRenderVersion),
                typeof(ChunkRenderPendingVersion),
                typeof(ChunkMeshDirty),
                typeof(ChunkMaterialVariant));

            entityManager.SetComponentData(presenter, new ChunkCoordComponent { X = 2, Y = 3 });
            entityManager.SetComponentData(presenter, new ChunkRenderVersion { Value = 5 });
            entityManager.SetComponentData(presenter, new ChunkRenderPendingVersion { Value = 5 });
            entityManager.SetComponentData(presenter, new ChunkMeshDirty { Mode = ChunkMeshDirtyMode.None });

            events.Add(new ChunkRenderInvalidationEvent
            {
                ChunkX = 2,
                ChunkY = 3,
                SnapshotVersion = 6,
                Mode = ChunkMeshDirtyMode.Rect,
                MinX = 4,
                MinY = 5,
                MaxX = 8,
                MaxY = 9
            });

            events.Add(new ChunkRenderInvalidationEvent
            {
                ChunkX = 2,
                ChunkY = 3,
                SnapshotVersion = 6,
                Mode = ChunkMeshDirtyMode.Rect,
                MinX = 1,
                MinY = 7,
                MaxX = 10,
                MaxY = 12
            });

            events.Add(new ChunkRenderInvalidationEvent
            {
                ChunkX = 2,
                ChunkY = 3,
                SnapshotVersion = 4,
                Mode = ChunkMeshDirtyMode.Full,
                MinX = 0,
                MinY = 0,
                MaxX = 0,
                MaxY = 0
            });

            system.Update();

            ChunkRenderPendingVersion pending = entityManager.GetComponentData<ChunkRenderPendingVersion>(presenter);
            ChunkMeshDirty dirty = entityManager.GetComponentData<ChunkMeshDirty>(presenter);
            if (pending.Value != 6)
            {
                return false;
            }

            if (dirty.Mode != ChunkMeshDirtyMode.Rect || dirty.MinX != 1 || dirty.MinY != 5 || dirty.MaxX != 10 || dirty.MaxY != 12)
            {
                return false;
            }

            events = entityManager.GetBuffer<ChunkRenderInvalidationEvent>(source);
            return events.Length == 0;
        }
    }
}
