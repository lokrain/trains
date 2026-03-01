#nullable enable
using Unity.Entities;

namespace OpenTTD.Presentation.WorldRendering
{
    /// <summary>
    /// Minimal self-test for AOI-driven chunk presenter spawn/despawn lifecycle.
    /// </summary>
    public static class ChunkPresenterLifecycleSelfTest
    {
        public static bool Run()
        {
            using var world = new World("ChunkPresenterLifecycleSelfTest");
            var entityManager = world.EntityManager;

            var lifecycle = world.CreateSystemManaged<ChunkPresenterLifecycleSystem>();

            Entity ready = entityManager.CreateEntity(typeof(ClientWorldReadyFence));
            entityManager.SetComponentData(ready, new ClientWorldReadyFence { IsReady = 0 });

            Entity aoi = entityManager.CreateEntity(typeof(ChunkPresenterAoiSource));
            DynamicBuffer<VisibleChunkElement> visible = entityManager.AddBuffer<VisibleChunkElement>(aoi);
            visible.Add(new VisibleChunkElement { X = 0, Y = 0 });
            visible.Add(new VisibleChunkElement { X = 1, Y = 0 });

            lifecycle.Update();

            EntityQuery q = entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<ChunkRenderTag>(),
                ComponentType.ReadOnly<ChunkCoordComponent>());

            if (q.CalculateEntityCount() != 0)
            {
                return false;
            }

            entityManager.SetComponentData(ready, new ClientWorldReadyFence { IsReady = 1 });
            lifecycle.Update();

            if (q.CalculateEntityCount() != 2)
            {
                return false;
            }

            DynamicBuffer<VisibleChunkElement> visible2 = entityManager.GetBuffer<VisibleChunkElement>(aoi);
            visible2.Clear();
            visible2.Add(new VisibleChunkElement { X = 1, Y = 0 });
            lifecycle.Update();

            if (q.CalculateEntityCount() != 1)
            {
                return false;
            }

            Entity only = q.GetSingletonEntity();
            ChunkCoordComponent coord = entityManager.GetComponentData<ChunkCoordComponent>(only);
            return coord.X == 1 && coord.Y == 0;
        }
    }
}
