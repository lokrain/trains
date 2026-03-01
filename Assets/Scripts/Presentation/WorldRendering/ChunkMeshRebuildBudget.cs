#nullable enable
using Unity.Entities;

namespace OpenTTD.Presentation.WorldRendering
{
    /// <summary>
    /// Per-frame budget for chunk render invalidation processing.
    /// </summary>
    public struct ChunkMeshRebuildBudget : IComponentData
    {
        public int MaxInvalidationsPerFrame;
    }

    /// <summary>
    /// Ensures a default rebuild budget singleton exists.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class ChunkMeshRebuildBudgetBootstrapSystem : SystemBase
    {
        protected override void OnStartRunning()
        {
            if (SystemAPI.TryGetSingletonEntity<ChunkMeshRebuildBudget>(out _))
            {
                return;
            }

            Entity e = EntityManager.CreateEntity(typeof(ChunkMeshRebuildBudget));
            EntityManager.SetComponentData(e, new ChunkMeshRebuildBudget
            {
                MaxInvalidationsPerFrame = 256
            });
        }

        protected override void OnUpdate()
        {
        }
    }
}
