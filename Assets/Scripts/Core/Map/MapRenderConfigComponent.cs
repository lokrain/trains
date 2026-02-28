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
    }
}
