#nullable enable
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using OpenTTD.Core.World;

namespace OpenTTD.Core.Terraform
{
    /// <summary>
    /// Applies a signed height delta over a chunk-local rectangular area.
    /// </summary>
    [BurstCompile]
    public struct ApplyTerraformRectJob : IJobParallelFor
    {
        public int ChunkX;
        public int ChunkY;

        public byte Rx;
        public byte Ry;
        public byte Rw;
        public byte Rh;
        public sbyte Delta;

        /// <summary>
        /// Target chunk height array.
        /// </summary>
        public NativeArray<byte> Height;

        public void Execute(int index)
        {
            int x = index % Rw;
            int y = index / Rw;
            int lx = Rx + x;
            int ly = Ry + y;

            int idx = WorldConstants.TileIndex(lx, ly);
            int v = Height[idx] + Delta;
            Height[idx] = (byte)math.clamp(v, 0, 255);
        }

        /// <summary>
        /// Number of tiles in the target rectangle.
        /// </summary>
        public int RectTileCount => Rw * Rh;
    }
}
