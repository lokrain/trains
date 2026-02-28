using Unity.Burst;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace OpenTTD.Core.Map
{
    /// <summary>
    /// Generates map tile data for entities configured with <see cref="MapGeneratorComponent" />.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct MapDataGenerationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var dependency = state.Dependency;

            foreach (var (mapSettings, entity) in SystemAPI.Query<RefRO<MapGeneratorComponent>>().WithNone<MapDataComponent>().WithEntityAccess())
            {
                var mapData = new NativeArray<TileData>(mapSettings.ValueRO.MapSize.x * mapSettings.ValueRO.MapSize.y, Allocator.Persistent);

                var generationJob = new GenerateMapJob
                {
                    MapSize = mapSettings.ValueRO.MapSize,
                    Seed = mapSettings.ValueRO.Seed,
                    NoiseScale = mapSettings.ValueRO.NoiseScale,
                    MapData = mapData
                };
                dependency = generationJob.Schedule(mapData.Length, 128, dependency);

                ecb.AddComponent(entity, new MapDataComponent { Map = mapData });
            }

            state.Dependency = dependency;
        }
    }

    /// <summary>
    /// Spawns visual tile entities for maps that have generated data and rendering configuration.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(MapDataGenerationSystem))]
    public partial struct MapVisualSpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entityEcb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            var spawnEcb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged)
                .AsParallelWriter();

            foreach (var (mapData, mapSettings, renderConfig, entity) in SystemAPI
                .Query<RefRO<MapDataComponent>, RefRO<MapGeneratorComponent>, RefRO<MapRenderConfigComponent>>()
                .WithNone<MapVisualsSpawnedTag>()
                .WithEntityAccess())
            {
                var spawnJob = new SpawnTileEntitiesJob
                {
                    MapSize = mapSettings.ValueRO.MapSize,
                    TilePrefab = renderConfig.ValueRO.TilePrefab,
                    MapData = mapData.ValueRO.Map,
                    Ecb = spawnEcb
                };

                state.Dependency = spawnJob.Schedule(mapData.ValueRO.Map.Length, 128, state.Dependency);

                entityEcb.AddComponent<MapVisualsSpawnedTag>(entity);
            }
        }
    }

    [BurstCompile]
    public struct GenerateMapJob : IJobParallelFor
    {
        public int2 MapSize;
        public int Seed;
        public float NoiseScale;
        public NativeArray<TileData> MapData;

        public void Execute(int index)
        {
            int x = index % MapSize.x;
            int y = index / MapSize.x;

            float noiseValue = noise.snoise(new float2(x / NoiseScale, y / NoiseScale) + new float2(Seed, Seed));
            byte height = (byte)math.remap(-1, 1, 0, 255, noiseValue);

            MapData[index] = new TileData
            {
                Height = height,
                Type = (byte)(height < 80 ? 1 : 0) // Simple water/grass based on height
            };
        }
    }

    [BurstCompile]
    public struct SpawnTileEntitiesJob : IJobParallelFor
    {
        public int2 MapSize;
        public Entity TilePrefab;
        [ReadOnly] public NativeArray<TileData> MapData;
        public EntityCommandBuffer.ParallelWriter Ecb;

        public void Execute(int index)
        {
            int x = index % MapSize.x;
            int y = index / MapSize.x;
            var tile = MapData[index];

            if (tile.Type == 1) return; // Don't render water tiles for now

            var newTileEntity = Ecb.Instantiate(index, TilePrefab);
            var position = new float3(x, tile.Height / 10f, y); // Scale height for visual effect

            Ecb.SetComponent(index, newTileEntity, LocalTransform.FromPosition(position));
        }
    }
}