using Unity.Entities; 

namespace OpenTTD.Core.Map
{
    /// <summary>
    /// Rendering configuration for map visualization entities.
    /// </summary>
    public struct MapRenderConfigComponent : IComponentData
    {
        /// <summary>
        /// Prefab used to instantiate visible land tiles.
        /// </summary>
        public Entity TilePrefab;

        /// <summary>
        /// Maximum number of chunk meshes built per frame during initial map mesh bootstrap.
        /// </summary>
        public int MaxChunkBuildsPerFrame;
    }
}
