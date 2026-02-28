#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace OpenTTD.Core.World
{
    /// <summary>
    /// Chunk-space coordinate in the world chunk grid.
    /// </summary>
    public readonly struct ChunkCoord : IEquatable<ChunkCoord>
    {
        /// <summary>
        /// Chunk X coordinate.
        /// </summary>
        public readonly short X;

        /// <summary>
        /// Chunk Y coordinate.
        /// </summary>
        public readonly short Y;

        /// <summary>
        /// Creates a chunk coordinate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChunkCoord(short x, short y)
        {
            X = x;
            Y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ChunkCoord other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkCoord other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (X & 0xFFFF) | (Y << 16);
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }

        /// <summary>
        /// Converts chunk coordinate to linear chunk index.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ToIndex()
        {
            return WorldConstants.ChunkIndex(X, Y);
        }

        /// <summary>
        /// Converts linear chunk index to chunk coordinate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ChunkCoord FromIndex(int idx)
        {
            return new ChunkCoord((short)(idx % WorldConstants.ChunksW), (short)(idx / WorldConstants.ChunksW));
        }

        public static bool operator ==(ChunkCoord left, ChunkCoord right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ChunkCoord left, ChunkCoord right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// Tile-space coordinate in world tile grid.
    /// </summary>
    public readonly struct TileCoord : IEquatable<TileCoord>
    {
        /// <summary>
        /// Tile X coordinate.
        /// </summary>
        public readonly ushort X;

        /// <summary>
        /// Tile Y coordinate.
        /// </summary>
        public readonly ushort Y;

        /// <summary>
        /// Creates a tile coordinate.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TileCoord(ushort x, ushort y)
        {
            X = x;
            Y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TileCoord other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is TileCoord other && Equals(other);
        }

        public override int GetHashCode()
        {
            return X | (Y << 16);
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }

        public static bool operator ==(TileCoord left, TileCoord right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TileCoord left, TileCoord right)
        {
            return !left.Equals(right);
        }
    }
}
