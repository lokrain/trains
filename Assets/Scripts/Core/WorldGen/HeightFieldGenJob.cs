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
        public WorldGenConfig Config;

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

            uint n0 = ValueNoise2D.SampleQ16(SeedHeight, xQ16, yQ16, Config.BaseGridTiles);
            uint n1 = ValueNoise2D.SampleQ16(SeedHeight + 1, xQ16, yQ16, Config.Octave1GridTiles);
            uint n2 = ValueNoise2D.SampleQ16(SeedHeight + 2, xQ16, yQ16, Config.Octave2GridTiles);
            uint n3 = ValueNoise2D.SampleQ16(SeedHeight + 3, xQ16, yQ16, Config.Octave3GridTiles);

            uint acc =
                (uint)(((ulong)n0 * Config.W0_Q16 + (ulong)n1 * Config.W1_Q16 + (ulong)n2 * Config.W2_Q16 + (ulong)n3 * Config.W3_Q16) >> 16);

            byte h = (byte)((acc >> 8) * Config.BaseAmplitude / 255u);

            switch (Config.HeightCurve)
            {
                case 1:
                {
                    uint hh = (uint)h * h;
                    h = (byte)(hh / 255u);
                    break;
                }
                case 2:
                {
                    uint hh = (uint)h * h;
                    uint hhh = hh * h;
                    h = (byte)(hhh / 65025u);
                    break;
                }
            }

            OutHeight[index] = h;
        }
    }
}
