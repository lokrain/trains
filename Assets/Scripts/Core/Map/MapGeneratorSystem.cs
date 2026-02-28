using Unity.Burst; 
using Unity.Collections;
using Unity.Entities;
using OpenTTD.Core.World;
using OpenTTD.Core.WorldGen;

namespace OpenTTD.Core.Map
{
    /// <summary>
    /// Generates map tile data for entities configured with <see cref="MapGeneratorComponent" />.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct MapDataGenerationSystem : ISystem
    {
        public readonly void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (mapSettings, entity) in SystemAPI.Query<RefRO<MapGeneratorComponent>>().WithNone<MapDataComponent>().WithEntityAccess())
            {
                ulong worldSeed = (ulong)mapSettings.ValueRO.Seed;
                WorldGenConfig config = WorldGenConfig.Default(worldSeed);
                WorldChunkArray world = WorldGenOrchestrator.GenerateWorld(config);

                ecb.AddComponent(entity, new MapDataComponent { World = world });
            }
        }
    }
}