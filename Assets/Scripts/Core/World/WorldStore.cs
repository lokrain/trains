#nullable enable
using System;
using Unity.Collections;

namespace OpenTTD.Core.World
{
    /// <summary>
    /// Central chunked world container with deterministic indexing and lifecycle ownership.
    /// </summary>
    public struct WorldStore : IDisposable
    {
        private WorldChunkArray _world;

        /// <summary>
        /// Returns true when underlying world storage is initialized.
        /// </summary>
        public bool IsCreated => _world.IsCreated;

        /// <summary>
        /// Current world sea level.
        /// </summary>
        public readonly byte SeaLevel => _world.SeaLevel;

        /// <summary>
        /// Creates world store for fixed 32x32 chunk map.
        /// </summary>
        public static WorldStore Create(byte seaLevel, Allocator allocator, Allocator chunkAllocator)
        {
            return new WorldStore
            {
                _world = WorldChunkArray.Create(seaLevel, allocator, chunkAllocator)
            };
        }

        /// <summary>
        /// Gets chunk by key.
        /// </summary>
        public ChunkSoA GetChunk(ChunkKey key)
        {
            return _world.GetChunk(key.ToIndex());
        }

        /// <summary>
        /// Gets chunk by index.
        /// </summary>
        public ChunkSoA GetChunk(int chunkIndex)
        {
            return _world.GetChunk(chunkIndex);
        }

        /// <summary>
        /// Sets chunk value by key.
        /// </summary>
        public void SetChunk(ChunkKey key, ChunkSoA chunk)
        {
            _world.SetChunk(key.ToIndex(), chunk);
        }

        /// <summary>
        /// Tries to read tile field view by world tile coordinate.
        /// </summary>
        public bool TryGetTile(TileCoord tile, out TileView view)
        {
            if (tile.X >= WorldConstants.MapW || tile.Y >= WorldConstants.MapH)
            {
                view = default;
                return false;
            }

            WorldIndexing.TileToChunkLocal(tile, out ChunkCoord chunkCoord, out LocalTileCoord local);
            int chunkIndex = chunkCoord.ToIndex();
            ChunkSoA chunk = _world.GetChunk(chunkIndex);
            int localIndex = WorldConstants.TileIndex(local.X, local.Y);
            view = new TileView(chunkIndex, localIndex, chunk.Height[localIndex], chunk.RiverMask[localIndex], chunk.Biome[localIndex], chunk.Slope[localIndex], chunk.BuildMask[localIndex]);
            return true;
        }

        /// <summary>
        /// Sets tile height field by world tile coordinate and marks chunk dirty.
        /// </summary>
        public bool SetTileHeight(TileCoord tile, byte height)
        {
            if (tile.X >= WorldConstants.MapW || tile.Y >= WorldConstants.MapH)
            {
                return false;
            }

            WorldIndexing.TileToChunkLocal(tile, out ChunkCoord chunkCoord, out LocalTileCoord local);
            int chunkIndex = chunkCoord.ToIndex();
            ChunkSoA chunk = _world.GetChunk(chunkIndex);
            int localIndex = WorldConstants.TileIndex(local.X, local.Y);
            chunk.Height[localIndex] = height;
            chunk.Dirty |= ChunkDirtyFlags.Height;
            chunk.MarkDirtyTile((byte)local.X, (byte)local.Y);
            _world.SetChunk(chunkIndex, chunk);
            return true;
        }

        /// <summary>
        /// Sets tile biome field by world tile coordinate and marks render/snapshot dirty.
        /// </summary>
        public bool SetTileBiome(TileCoord tile, byte biome)
        {
            if (tile.X >= WorldConstants.MapW || tile.Y >= WorldConstants.MapH)
            {
                return false;
            }

            WorldIndexing.TileToChunkLocal(tile, out ChunkCoord chunkCoord, out LocalTileCoord local);
            int chunkIndex = chunkCoord.ToIndex();
            ChunkSoA chunk = _world.GetChunk(chunkIndex);
            int localIndex = WorldConstants.TileIndex(local.X, local.Y);
            chunk.Biome[localIndex] = biome;
            chunk.Dirty |= ChunkDirtyFlags.Snapshot | ChunkDirtyFlags.Render;
            chunk.MarkDirtyTile((byte)local.X, (byte)local.Y);
            _world.SetChunk(chunkIndex, chunk);
            return true;
        }

        /// <summary>
        /// Returns underlying world chunk array.
        /// </summary>
        public WorldChunkArray AsChunkArray()
        {
            return _world;
        }

        public void Dispose()
        {
            _world.Dispose();
        }
    }

    /// <summary>
    /// Read-only tile field view.
    /// </summary>
    public readonly struct TileView
    {
        public readonly int ChunkIndex;
        public readonly int LocalIndex;
        public readonly byte Height;
        public readonly byte RiverMask;
        public readonly byte Biome;
        public readonly byte Slope;
        public readonly ushort BuildMask;

        public TileView(int chunkIndex, int localIndex, byte height, byte riverMask, byte biome, byte slope, ushort buildMask)
        {
            ChunkIndex = chunkIndex;
            LocalIndex = localIndex;
            Height = height;
            RiverMask = riverMask;
            Biome = biome;
            Slope = slope;
            BuildMask = buildMask;
        }
    }
}
