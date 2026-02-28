#nullable enable

namespace OpenTTD.Core.World
{
    /// <summary>
    /// Read-only chunk field access boundary.
    /// </summary>
    public interface IChunkRead
    {
        byte SeaLevel { get; }
        bool TryGetChunk(ChunkKey key, out ChunkSoA chunk);
        bool TryGetTile(TileCoord tile, out TileView tileView);
    }

    /// <summary>
    /// Read-only world access boundary used by simulation subsystems.
    /// </summary>
    public interface IWorldRead : IChunkRead
    {
        byte GetHeightClamped(int x, int y);
        bool IsRiver(int x, int y);
        byte GetBiome(int x, int y);
    }

    /// <summary>
    /// Mutable world access boundary used by authoritative simulation services.
    /// </summary>
    public interface IWorldWrite : IWorldRead
    {
        void SetHeight(int x, int y, byte height);
        void SetChunkDirty(int chunkX, int chunkY, ChunkDirtyFlags flags);
        void SetBiome(int x, int y, byte biome);
    }

    /// <summary>
    /// Temporary adapter over current chunked world representation.
    /// Enables phased migration from direct data access to interface-bound world services.
    /// </summary>
    public sealed class WorldChunkArrayAdapter : IWorldWrite
    {
        private WorldChunkArray _world;

        public WorldChunkArrayAdapter(in WorldChunkArray world)
        {
            _world = world;
        }

        public byte SeaLevel => _world.SeaLevel;

        public bool TryGetChunk(ChunkKey key, out ChunkSoA chunk)
        {
            int idx = key.ToIndex();
            if ((uint)idx >= WorldConstants.ChunkCount)
            {
                chunk = default;
                return false;
            }

            chunk = _world.GetChunk(idx);
            return true;
        }

        public bool TryGetTile(TileCoord tile, out TileView tileView)
        {
            if (tile.X >= WorldConstants.MapW || tile.Y >= WorldConstants.MapH)
            {
                tileView = default;
                return false;
            }

            WorldIndexing.TileToChunkLocal(tile, out ChunkCoord chunkCoord, out LocalTileCoord local);
            int chunkIndex = chunkCoord.ToIndex();
            ChunkSoA chunk = _world.GetChunk(chunkIndex);
            int localIndex = WorldConstants.TileIndex(local.X, local.Y);
            tileView = new TileView(
                chunkIndex,
                localIndex,
                chunk.Height[localIndex],
                chunk.RiverMask[localIndex],
                chunk.Biome[localIndex],
                chunk.Slope[localIndex],
                chunk.BuildMask[localIndex]);
            return true;
        }

        public byte GetHeightClamped(int x, int y)
        {
            return TileAccessor.GetHeightClamped(ref _world, x, y);
        }

        public bool IsRiver(int x, int y)
        {
            x = WorldConstants.ClampMapX(x);
            y = WorldConstants.ClampMapY(y);
            TileAccessor.WorldToChunkLocal(x, y, out int cx, out int cy, out int lx, out int ly);
            ChunkSoA chunk = _world.GetChunk(cx, cy);
            return chunk.RiverMask[WorldConstants.TileIndex(lx, ly)] != 0;
        }

        public byte GetBiome(int x, int y)
        {
            x = WorldConstants.ClampMapX(x);
            y = WorldConstants.ClampMapY(y);
            TileAccessor.WorldToChunkLocal(x, y, out int cx, out int cy, out int lx, out int ly);
            ChunkSoA chunk = _world.GetChunk(cx, cy);
            return chunk.Biome[WorldConstants.TileIndex(lx, ly)];
        }

        public void SetHeight(int x, int y, byte height)
        {
            x = WorldConstants.ClampMapX(x);
            y = WorldConstants.ClampMapY(y);
            TileAccessor.WorldToChunkLocal(x, y, out int cx, out int cy, out int lx, out int ly);
            ChunkSoA chunk = _world.GetChunk(cx, cy);
            chunk.Height[WorldConstants.TileIndex(lx, ly)] = height;
            chunk.Dirty |= ChunkDirtyFlags.Height;
            _world.SetChunk(cx, cy, chunk);
        }

        public void SetChunkDirty(int chunkX, int chunkY, ChunkDirtyFlags flags)
        {
            ChunkSoA chunk = _world.GetChunk(chunkX, chunkY);
            chunk.Dirty |= flags;
            _world.SetChunk(chunkX, chunkY, chunk);
        }

        public void SetBiome(int x, int y, byte biome)
        {
            x = WorldConstants.ClampMapX(x);
            y = WorldConstants.ClampMapY(y);
            TileAccessor.WorldToChunkLocal(x, y, out int cx, out int cy, out int lx, out int ly);
            ChunkSoA chunk = _world.GetChunk(cx, cy);
            int localIndex = WorldConstants.TileIndex(lx, ly);
            chunk.Biome[localIndex] = biome;
            chunk.Dirty |= ChunkDirtyFlags.Snapshot | ChunkDirtyFlags.Render;
            chunk.MarkDirtyTile((byte)lx, (byte)ly);
            _world.SetChunk(cx, cy, chunk);
        }

        /// <summary>
        /// Returns the adapted world value with all applied writes.
        /// </summary>
        public WorldChunkArray IntoWorld()
        {
            return _world;
        }
    }
}
