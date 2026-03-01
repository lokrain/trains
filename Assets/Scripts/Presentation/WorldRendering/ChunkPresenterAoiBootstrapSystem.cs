#nullable enable
using Unity.Entities;

namespace OpenTTD.Presentation.WorldRendering
{
    /// <summary>
    /// Ensures a singleton AOI source entity exists for chunk presenter lifecycle input.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class ChunkPresenterAoiBootstrapSystem : SystemBase
    {
        protected override void OnStartRunning()
        {
            if (SystemAPI.TryGetSingletonEntity<ChunkPresenterAoiSource>(out _))
            {
                return;
            }

            Entity e = EntityManager.CreateEntity(typeof(ChunkPresenterAoiSource));
            EntityManager.AddBuffer<VisibleChunkElement>(e);
        }

        protected override void OnUpdate()
        {
        }
    }
}
