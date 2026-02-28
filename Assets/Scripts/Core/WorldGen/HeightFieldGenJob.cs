#nullable enable
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace OpenTTD.Core.WorldGen
{
    /// <summary>
    /// Generates chunk heights using deterministic integer value noise (FBM-ish).
    /// Outputs u8 height.
    /// </summary>
    [BurstCompile]
    public struct HeightFieldGenJob : IJobParallelFor
    {
        public ulong SeedHeight;

        /// <summary>
        /// Chunk X coordinate in [0..31].
        /// </summary>
        public int ChunkX;

        /// <summary>
        /// Chunk Y coordinate in [0..31].
        /// </summary>
        public int ChunkY;

        /// <summary>
        /// Output height array of length 4096.
        /// </summary>
        public NativeArray<byte> OutHeight;

        public void Execute(int index)
        {
            int lx = index & 63;
            int ly = index >> 6;

            int x = (ChunkX << 6) + lx;
            int y = (ChunkY << 6) + ly;

            int xQ16 = x << 16;
            int yQ16 = y << 16;

            uint n0 = ValueNoise2D.SampleQ16(SeedHeight, xQ16, yQ16, gridSizeTiles: 256);
            uint n1 = ValueNoise2D.SampleQ16(SeedHeight + 1, xQ16, yQ16, gridSizeTiles: 128);
            uint n2 = ValueNoise2D.SampleQ16(SeedHeight + 2, xQ16, yQ16, gridSizeTiles: 64);
            uint n3 = ValueNoise2D.SampleQ16(SeedHeight + 3, xQ16, yQ16, gridSizeTiles: 32);

            uint acc =
                (uint)(((ulong)n0 * 29491ul + (ulong)n1 * 16384ul + (ulong)n2 * 11796ul + (ulong)n3 * 7864ul) >> 16);

            byte h = (byte)(acc >> 8);

            uint hh = (uint)h * h;
            h = (byte)(hh / 255u);

            OutHeight[index] = h;
        }
    }
}
