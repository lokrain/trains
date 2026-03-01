#nullable enable
using Unity.Entities;

namespace OpenTTD.Presentation.WorldRendering
{
    /// <summary>
    /// Ensures a singleton invalidation source entity exists for chunk render dirty events.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class ChunkRenderInvalidationBootstrapSystem : SystemBase
    {
        protected override void OnStartRunning()
        {
            if (SystemAPI.TryGetSingletonEntity<ChunkRenderInvalidationSource>(out _))
            {
                return;
            }

            Entity e = EntityManager.CreateEntity(typeof(ChunkRenderInvalidationSource));
            EntityManager.AddBuffer<ChunkRenderInvalidationEvent>(e);
        }

        protected override void OnUpdate()
        {
        }
    }
}
