#nullable enable
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace OpenTTD.Core.World
{
    /// <summary>
    /// Burst-friendly tile access helpers with neighbor reads across chunk boundaries.
    /// Reads clamp to world bounds.
    /// </summary>
    public static class TileAccessor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WorldToChunkLocal(int x, int y, out int cx, out int cy, out int lx, out int ly)
        {
            cx = x >> WorldConstants.ChunkShift;
            cy = y >> WorldConstants.ChunkShift;
            lx = x & WorldConstants.ChunkMask;
            ly = y & WorldConstants.ChunkMask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte GetHeightClamped(ref WorldChunkArray world, int x, int y)
        {
            x = WorldConstants.ClampMapX(x);
            y = WorldConstants.ClampMapY(y);

            WorldToChunkLocal(x, y, out int cx, out int cy, out int lx, out int ly);
            ChunkSoA c = world.GetChunk(cx, cy);
            return c.Height[WorldConstants.TileIndex(lx, ly)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSea(ref WorldChunkArray world, ref ChunkSoA c, int lx, int ly)
        {
            int idx = WorldConstants.TileIndex(lx, ly);
            if (c.RiverMask[idx] != 0)
            {
                return false;
            }

            return c.Height[idx] <= world.SeaLevel;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRiver(ref ChunkSoA c, int lx, int ly)
        {
            return c.RiverMask[WorldConstants.TileIndex(lx, ly)] != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ComputeSlopeClass(byte hC, byte hN, byte hS, byte hE, byte hW, byte class1MaxDelta = 1, byte class2MaxDelta = 3, byte class3MaxDelta = 6)
        {
            int dn = math.abs(hC - hN);
            int ds = math.abs(hC - hS);
            int de = math.abs(hC - hE);
            int dw = math.abs(hC - hW);
            int d = math.max(math.max(dn, ds), math.max(de, dw));

            if (d <= class1MaxDelta)
            {
                return 0;
            }

            if (d <= class2MaxDelta)
            {
                return 1;
            }

            if (d <= class3MaxDelta)
            {
                return 2;
            }

            return 3;
        }
    }
}
