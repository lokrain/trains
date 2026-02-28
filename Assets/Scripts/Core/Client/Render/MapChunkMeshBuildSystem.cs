#nullable enable
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using OpenTTD.Core.Map;
using OpenTTD.Core.World;

namespace OpenTTD.Core.Client.Render
{
    /// <summary>
    /// Builds chunk meshes for maps that have chunked world data and render config.
    /// Per-tile entity visual spawning is replaced by chunk mesh generation.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(OpenTTD.Core.Map.MapDataGenerationSystem))]
    public partial class MapChunkMeshBuildSystem : SystemBase
    {
        private readonly Dictionary<ulong, Mesh> _chunkMeshes = new Dictionary<ulong, Mesh>();

        protected override void OnDestroy()
        {
            foreach (var kv in _chunkMeshes)
            {
                if (kv.Value != null)
                {
                    Object.Destroy(kv.Value);
                }
            }

            _chunkMeshes.Clear();
        }

        protected override void OnUpdate()
        {
            var entityManager = EntityManager;

            foreach (var (mapData, entity) in SystemAPI.Query<RefRW<MapDataComponent>>().WithAll<MapRenderConfigComponent>().WithNone<MapChunkMeshesBuiltTag>().WithEntityAccess())
            {
                for (int chunkIndex = 0; chunkIndex < WorldConstants.ChunkCount; chunkIndex++)
                {
                    ulong key = ((ulong)(uint)entity.Index << 32) | (uint)chunkIndex;

                    Mesh existing;
                    _chunkMeshes.TryGetValue(key, out existing);

                    ChunkCoord cc = ChunkCoord.FromIndex(chunkIndex);
                    Mesh updated = ChunkMeshBuilder.BuildOrUpdate(existing, ref mapData.ValueRW.World, cc.X, cc.Y);
                    _chunkMeshes[key] = updated;
                }

                entityManager.AddComponent<MapChunkMeshesBuiltTag>(entity);
            }
        }
    }
}
