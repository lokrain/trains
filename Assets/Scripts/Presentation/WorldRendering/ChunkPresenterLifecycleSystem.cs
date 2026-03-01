#nullable enable
using Unity.Collections;
using Unity.Entities;

namespace OpenTTD.Presentation.WorldRendering
{
    /// <summary>
    /// Maintains one presenter entity per currently visible chunk from AOI buffer input.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class ChunkPresenterLifecycleSystem : SystemBase
    {
        private const int ChunksW = 32;
        private const int ChunksH = 32;
        private const int ChunkSize = 64;
        private const int ChunkCount = ChunksW * ChunksH;

        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingleton<ClientWorldReadyFence>(out ClientWorldReadyFence readyFence) || readyFence.IsReady == 0)
            {
                return;
            }

            if (!SystemAPI.TryGetSingletonEntity<ChunkPresenterAoiSource>(out Entity aoiEntity))
            {
                return;
            }

            DynamicBuffer<VisibleChunkElement> visible = EntityManager.GetBuffer<VisibleChunkElement>(aoiEntity);
            var desired = new NativeParallelHashSet<int>(visible.Length, Allocator.Temp);
            var existing = new NativeParallelHashSet<int>(ChunkCount, Allocator.Temp);

            for (int i = 0; i < visible.Length; i++)
            {
                short cx = visible[i].X;
                short cy = visible[i].Y;
                if ((uint)cx >= ChunksW || (uint)cy >= ChunksH)
                {
                    continue;
                }

                desired.Add(ChunkIndex(cx, cy));
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (coord, entity) in SystemAPI.Query<RefRO<ChunkCoordComponent>>().WithAll<ChunkRenderTag>().WithEntityAccess())
            {
                short cx = coord.ValueRO.X;
                short cy = coord.ValueRO.Y;
                if ((uint)cx >= ChunksW || (uint)cy >= ChunksH)
                {
                    ecb.DestroyEntity(entity);
                    continue;
                }

                int index = ChunkIndex(cx, cy);
                existing.Add(index);

                if (!desired.Contains(index))
                {
                    ecb.DestroyEntity(entity);
                }
            }

            foreach (int index in desired)
            {
                if (existing.Contains(index))
                {
                    continue;
                }

                short chunkX = (short)(index % ChunksW);
                short chunkY = (short)(index / ChunksW);
                Entity presenter = EntityManager.CreateEntity();
                EntityManager.AddComponent<ChunkRenderTag>(presenter);
                EntityManager.AddComponentData(presenter, new ChunkCoordComponent { X = chunkX, Y = chunkY });
                EntityManager.AddComponentData(presenter, new ChunkRenderVersion { Value = 0 });
                EntityManager.AddComponentData(presenter, new ChunkRenderPendingVersion { Value = 0 });
                EntityManager.AddComponentData(presenter, new ChunkMeshDirty
                {
                    Mode = ChunkMeshDirtyMode.Full,
                    MinX = 0,
                    MinY = 0,
                    MaxX = (byte)(ChunkSize - 1),
                    MaxY = (byte)(ChunkSize - 1)
                });
                EntityManager.AddComponentData(presenter, new ChunkMaterialVariant { Value = 0 });
                EntityManager.AddComponent<ChunkRenderBindingPending>(presenter);
            }
        }

        private static int ChunkIndex(int cx, int cy)
        {
            return cx + cy * ChunksW;
        }
    }
}
