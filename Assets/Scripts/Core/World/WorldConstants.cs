#nullable enable
using System.Runtime.CompilerServices;

namespace OpenTTD.Core.World
{
    /// <summary>
    /// Shared world dimension and indexing constants.
    /// </summary>
    public static class WorldConstants
    {
        public const int MapW = 2048;
        public const int MapH = 2048;

        public const int ChunkSize = 64;
        public const int ChunkShift = 6; // 2^6 = 64
        public const int ChunkMask = ChunkSize - 1;

        public const int ChunksW = MapW / ChunkSize; // 32
        public const int ChunksH = MapH / ChunkSize; // 32
        public const int ChunkCount = ChunksW * ChunksH; // 1024

        /// <summary>
        /// Height is u8. Sea is derived: height &lt;= SeaLevel and not river.
        /// </summary>
        public const byte DefaultSeaLevel = 96;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ChunkIndex(int cx, int cy)
        {
            return cx + cy * ChunksW;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TileIndex(int lx, int ly)
        {
            return lx + ly * ChunkSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ClampMapX(int x)
        {
            return x < 0 ? 0 : (x >= MapW ? MapW - 1 : x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ClampMapY(int y)
        {
            return y < 0 ? 0 : (y >= MapH ? MapH - 1 : y);
        }
    }
}
