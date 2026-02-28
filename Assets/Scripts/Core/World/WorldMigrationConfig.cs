#nullable enable
using Unity.Entities;

namespace OpenTTD.Core.World
{
    /// <summary>
    /// Runtime migration feature flags for phased world backend rollout.
    /// </summary>
    public struct WorldMigrationConfig : IComponentData
    {
        /// <summary>
        /// When enabled, systems should route world reads through WorldStoreV2 interfaces.
        /// </summary>
        public bool UseWorldStoreV2;
    }
}
