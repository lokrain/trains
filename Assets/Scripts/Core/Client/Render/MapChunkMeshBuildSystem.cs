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
        private readonly Dictionary<ulong, Mesh> _chunkMeshes = new();

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

            foreach (var (mapDataRead, renderConfigRead, entity) in SystemAPI.Query<RefRO<MapDataComponent>, RefRO<MapRenderConfigComponent>>().WithNone<MapChunkMeshBuildProgress, MapChunkMeshesBuiltTag>().WithEntityAccess())
            {
                _ = mapDataRead;
                _ = renderConfigRead;
                entityManager.AddComponentData(entity, new MapChunkMeshBuildProgress { NextChunkIndex = 0 });
            }

            foreach (var (mapData, renderConfig, progress, entity) in SystemAPI.Query<RefRW<MapDataComponent>, RefRO<MapRenderConfigComponent>, RefRW<MapChunkMeshBuildProgress>>().WithNone<MapChunkMeshesBuiltTag>().WithEntityAccess())
            {
                int maxBuilds = renderConfig.ValueRO.MaxChunkBuildsPerFrame;
                if (maxBuilds <= 0)
                {
                    maxBuilds = 32;
                }

                int next = progress.ValueRO.NextChunkIndex;
                int end = next + maxBuilds;
                if (end > WorldConstants.ChunkCount)
                {
                    end = WorldConstants.ChunkCount;
                }

                for (int chunkIndex = next; chunkIndex < end; chunkIndex++)
                {
                    ulong key = ((ulong)(uint)entity.Index << 32) | (uint)chunkIndex;

                    _chunkMeshes.TryGetValue(key, out Mesh existing);

                    ChunkCoord cc = ChunkCoord.FromIndex(chunkIndex);
                    Mesh updated = ChunkMeshBuilder.BuildOrUpdate(existing, ref mapData.ValueRW.World, cc.X, cc.Y);
                    _chunkMeshes[key] = updated;
                }

                progress.ValueRW.NextChunkIndex = end;
                if (end >= WorldConstants.ChunkCount)
                {
                    entityManager.AddComponent<MapChunkMeshesBuiltTag>(entity);
                    entityManager.RemoveComponent<MapChunkMeshBuildProgress>(entity);
                }
            }

            foreach (var (mapData, renderConfig, entity) in SystemAPI.Query<RefRW<MapDataComponent>, RefRO<MapRenderConfigComponent>>().WithAll<MapChunkMeshesBuiltTag>().WithEntityAccess())
            {
                int maxBuilds = renderConfig.ValueRO.MaxChunkBuildsPerFrame;
                if (maxBuilds <= 0)
                {
                    maxBuilds = 32;
                }

                int builtThisFrame = 0;
                for (int chunkIndex = 0; chunkIndex < WorldConstants.ChunkCount && builtThisFrame < maxBuilds; chunkIndex++)
                {
                    ChunkSoA chunk = mapData.ValueRW.World.GetChunk(chunkIndex);
                    if ((chunk.Dirty & ChunkDirtyFlags.Render) == 0)
                    {
                        continue;
                    }

                    ulong key = ((ulong)(uint)entity.Index << 32) | (uint)chunkIndex;
                    _chunkMeshes.TryGetValue(key, out Mesh existing);

                    ChunkCoord cc = ChunkCoord.FromIndex(chunkIndex);
                    Mesh updated;
                    if (chunk.DirtyMinX != byte.MaxValue)
                    {
                        updated = ChunkMeshBuilder.BuildOrUpdateRect(
                            existing,
                            ref mapData.ValueRW.World,
                            cc.X,
                            cc.Y,
                            chunk.DirtyMinX,
                            chunk.DirtyMinY,
                            chunk.DirtyMaxX,
                            chunk.DirtyMaxY);
                    }
                    else
                    {
                        updated = ChunkMeshBuilder.BuildOrUpdate(existing, ref mapData.ValueRW.World, cc.X, cc.Y);
                    }

                    _chunkMeshes[key] = updated;

                    chunk.Dirty &= ~ChunkDirtyFlags.Render;
                    chunk.ClearDirtyRect();
                    mapData.ValueRW.World.SetChunk(chunkIndex, chunk);
                    builtThisFrame++;
                }
            }
        }
    }
}
