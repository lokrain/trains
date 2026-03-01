using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace OpenTTD.Core.Map
{
    /// <summary>
    /// Authoring component for configuring map generation settings.
    /// </summary>
    public class MapAuthoring : MonoBehaviour
    {
        [Header("Map Settings")]
        [Tooltip("Map size in tiles (x = width, y = height)")]
        public int2 MapSize = new(2048, 2048);

        [Tooltip("Deterministic seed used for terrain noise generation")]
        public int Seed = 12345;

        [Tooltip("Noise sampling scale. Higher values produce smoother terrain")]
        [Min(0.001f)]
        public float NoiseScale = 50f;

        [Header("Visualization")]
        [Tooltip("Prefab used to render generated land tiles")]
        public GameObject TilePrefab;

        [Tooltip("Maximum chunk meshes to build per frame during initial mesh bootstrap")]
        [Min(1)]
        public int MaxChunkBuildsPerFrame = 32;

        private void OnValidate()
        {
            MapSize.x = math.max(1, MapSize.x);
            MapSize.y = math.max(1, MapSize.y);
            NoiseScale = math.max(0.001f, NoiseScale);
            MaxChunkBuildsPerFrame = math.max(1, MaxChunkBuildsPerFrame);
        }
    }

    /// <summary>
    /// Bakes <see cref="MapAuthoring" /> into <see cref="MapGeneratorComponent" /> data.
    /// </summary>
    public class MapBaker : Baker<MapAuthoring>
    {
        /// <summary>
        /// Converts authoring settings into ECS component data.
        /// </summary>
        /// <param name="authoring">Map authoring settings source.</param>
        public override void Bake(MapAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            var tilePrefabEntity = authoring.TilePrefab != null
                ? GetEntity(authoring.TilePrefab, TransformUsageFlags.Dynamic)
                : Entity.Null;

            AddComponent(entity, new MapGeneratorComponent
            {
                MapSize = authoring.MapSize,
                Seed = authoring.Seed,
                NoiseScale = authoring.NoiseScale
            });

            if (tilePrefabEntity != Entity.Null)
            {
                AddComponent(entity, new MapRenderConfigComponent
                {
                    TilePrefab = tilePrefabEntity,
                    MaxChunkBuildsPerFrame = authoring.MaxChunkBuildsPerFrame
                });
            }
        }
    }
}