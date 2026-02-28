#nullable enable

namespace OpenTTD.Core.World
{
    /// <summary>
    /// Version counters for authoritative and derived per-chunk data.
    /// </summary>
    public struct ChunkVersions
    {
        /// <summary>
        /// Increments when authoritative height data changes.
        /// </summary>
        public uint HeightVersion;

        /// <summary>
        /// Increments when derived fields (e.g., slope/build mask) are recomputed.
        /// </summary>
        public uint DerivedVersion;

        /// <summary>
        /// Increments when snapshot payload state changes for replication.
        /// </summary>
        public uint SnapshotVersion;

        /// <summary>
        /// Increments when client-side render-relevant data changes.
        /// </summary>
        public uint RenderVersion;
    }
}
