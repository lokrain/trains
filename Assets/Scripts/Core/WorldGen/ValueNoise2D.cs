#nullable enable
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace OpenTTD.Core.WorldGen
{
    /// <summary>
    /// Deterministic value noise using hashed lattice corners plus fixed-point interpolation.
    /// No floating-point math is required for authoritative outputs.
    /// </summary>
    public static class ValueNoise2D
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint SmoothQ16(uint t)
        {
            ulong tt = (ulong)t * t >> 16;
            ulong ttt = tt * t >> 16;
            ulong a = 3ul * tt;
            ulong b = 2ul * ttt;
            long s = (long)a - (long)b;
            return (uint)math.clamp(s, 0, 65535);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint LerpQ16(uint a, uint b, uint tQ16)
        {
            return (uint)(((ulong)a * (65535u - tQ16) + (ulong)b * tQ16) >> 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint CornerQ16(ulong seed, int gx, int gy)
        {
            ulong h = Hash64.Hash(seed, (ulong)(uint)gx, (ulong)(uint)gy);
            return (uint)(h & 0xFFFFu);
        }

        /// <summary>
        /// Samples deterministic value noise at fixed-point coordinates.
        /// Coordinates are Q16 where 1.0 tile = 65536.
        /// </summary>
        /// <param name="seed">Noise seed.</param>
        /// <param name="xQ16">X coordinate in Q16.</param>
        /// <param name="yQ16">Y coordinate in Q16.</param>
        /// <param name="gridSizeTiles">Lattice grid size in tiles.</param>
        /// <returns>Noise value in Q16 range [0..65535].</returns>
        public static uint SampleQ16(ulong seed, int xQ16, int yQ16, int gridSizeTiles)
        {
            int cellQ16 = gridSizeTiles << 16;

            int gx0 = xQ16 / cellQ16;
            int gy0 = yQ16 / cellQ16;

            int gx1 = gx0 + 1;
            int gy1 = gy0 + 1;

            uint tx = (uint)math.clamp((xQ16 - gx0 * cellQ16) * 65535 / cellQ16, 0, 65535);
            uint ty = (uint)math.clamp((yQ16 - gy0 * cellQ16) * 65535 / cellQ16, 0, 65535);

            tx = SmoothQ16(tx);
            ty = SmoothQ16(ty);

            uint c00 = CornerQ16(seed, gx0, gy0);
            uint c10 = CornerQ16(seed, gx1, gy0);
            uint c01 = CornerQ16(seed, gx0, gy1);
            uint c11 = CornerQ16(seed, gx1, gy1);

            uint x0 = LerpQ16(c00, c10, tx);
            uint x1 = LerpQ16(c01, c11, tx);
            return LerpQ16(x0, x1, ty);
        }
    }
}
