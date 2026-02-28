#nullable enable
using Unity.Entities;

namespace OpenTTD.Core.World
{
    /// <summary>
    /// Ensures migration feature flags singleton exists.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public partial struct WorldMigrationConfigBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            EntityQuery query = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<WorldMigrationConfig>());
            if (query.IsEmptyIgnoreFilter)
            {
                Entity entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(entity, new WorldMigrationConfig
                {
                    UseWorldStoreV2 = true
                });
            }
        }

        public readonly void OnUpdate(ref SystemState state)
        {
        }
    }
}
