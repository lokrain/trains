#nullable enable

namespace OpenTTD.Presentation.WorldRendering
{
    /// <summary>
    /// Immutable chunk render metadata contract.
    /// </summary>
    public readonly struct ChunkRenderMeta
    {
        public readonly uint SnapshotVersion;
        public readonly uint DirtyFlags;
        public readonly byte DirtyMinX;
        public readonly byte DirtyMinY;
        public readonly byte DirtyMaxX;
        public readonly byte DirtyMaxY;

        public ChunkRenderMeta(
            uint snapshotVersion,
            uint dirtyFlags,
            byte dirtyMinX,
            byte dirtyMinY,
            byte dirtyMaxX,
            byte dirtyMaxY)
        {
            SnapshotVersion = snapshotVersion;
            DirtyFlags = dirtyFlags;
            DirtyMinX = dirtyMinX;
            DirtyMinY = dirtyMinY;
            DirtyMaxX = dirtyMaxX;
            DirtyMaxY = dirtyMaxY;
        }
    }

    /// <summary>
    /// Read-only rendering input contract over chunk world data.
    /// </summary>
    public interface IChunkRenderRead
    {
        byte SeaLevel { get; }

        bool TryGetChunkMeta(short chunkX, short chunkY, out ChunkRenderMeta meta);

        bool TryGetTileFields(int worldX, int worldY, out byte height, out byte riverMask, out byte biome, out byte slope);
    }
}
