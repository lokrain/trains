#nullable enable
using Unity.Entities;

namespace OpenTTD.Presentation.WorldRendering
{
    /// <summary>
    /// Minimal self-test for invalidation budget limiting and queue carryover.
    /// </summary>
    public static class ChunkRenderBudgetSelfTest
    {
        public static bool Run()
        {
            using var world = new World("ChunkRenderBudgetSelfTest");
            var entityManager = world.EntityManager;
            var system = world.CreateSystemManaged<ChunkRenderInvalidationSystem>();

            Entity budgetEntity = entityManager.CreateEntity(typeof(ChunkMeshRebuildBudget));
            entityManager.SetComponentData(budgetEntity, new ChunkMeshRebuildBudget { MaxInvalidationsPerFrame = 1 });

            Entity source = entityManager.CreateEntity(typeof(ChunkRenderInvalidationSource));
            DynamicBuffer<ChunkRenderInvalidationEvent> events = entityManager.AddBuffer<ChunkRenderInvalidationEvent>(source);

            Entity presenter = entityManager.CreateEntity(
                typeof(ChunkRenderTag),
                typeof(ChunkCoordComponent),
                typeof(ChunkRenderVersion),
                typeof(ChunkRenderPendingVersion),
                typeof(ChunkMeshDirty),
                typeof(ChunkMaterialVariant));

            entityManager.SetComponentData(presenter, new ChunkCoordComponent { X = 1, Y = 1 });
            entityManager.SetComponentData(presenter, new ChunkRenderVersion { Value = 0 });
            entityManager.SetComponentData(presenter, new ChunkRenderPendingVersion { Value = 0 });
            entityManager.SetComponentData(presenter, new ChunkMeshDirty { Mode = ChunkMeshDirtyMode.None });

            events.Add(new ChunkRenderInvalidationEvent
            {
                ChunkX = 1,
                ChunkY = 1,
                SnapshotVersion = 1,
                Mode = ChunkMeshDirtyMode.Rect,
                MinX = 2,
                MinY = 2,
                MaxX = 3,
                MaxY = 3
            });

            events.Add(new ChunkRenderInvalidationEvent
            {
                ChunkX = 1,
                ChunkY = 1,
                SnapshotVersion = 2,
                Mode = ChunkMeshDirtyMode.Rect,
                MinX = 4,
                MinY = 4,
                MaxX = 5,
                MaxY = 5
            });

            system.Update();

            DynamicBuffer<ChunkRenderInvalidationEvent> afterFirst = entityManager.GetBuffer<ChunkRenderInvalidationEvent>(source);
            if (afterFirst.Length != 1)
            {
                return false;
            }

            ChunkRenderPendingVersion pending = entityManager.GetComponentData<ChunkRenderPendingVersion>(presenter);
            if (pending.Value != 1)
            {
                return false;
            }

            system.Update();
            DynamicBuffer<ChunkRenderInvalidationEvent> afterSecond = entityManager.GetBuffer<ChunkRenderInvalidationEvent>(source);
            pending = entityManager.GetComponentData<ChunkRenderPendingVersion>(presenter);
            return afterSecond.Length == 0 && pending.Value == 2;
        }
    }
}
