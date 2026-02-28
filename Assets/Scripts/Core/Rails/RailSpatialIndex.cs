using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Authoritative tile-edge to segment index.
    /// </summary>
    public struct RailSpatialIndex : System.IDisposable
    {
        private NativeParallelHashMap<uint, SegmentId> _edgeToSeg;

        /// <summary>
        /// Creates a spatial index with initial capacity.
        /// </summary>
        /// <param name="capacity">Initial hash map capacity.</param>
        /// <param name="alloc">Allocator for native storage.</param>
        /// <returns>Initialized index.</returns>
        public static RailSpatialIndex Create(int capacity, Allocator alloc)
        {
            return new RailSpatialIndex
            {
                _edgeToSeg = new NativeParallelHashMap<uint, SegmentId>(capacity, alloc)
            };
        }

        /// <summary>
        /// Tries to add a mapping from tile edge key to segment id.
        /// </summary>
        public bool TryAdd(uint key, SegmentId segmentId)
        {
            return _edgeToSeg.TryAdd(key, segmentId);
        }

        /// <summary>
        /// Tries to resolve a tile edge key to a segment id.
        /// </summary>
        public bool TryGet(uint key, out SegmentId segmentId)
        {
            return _edgeToSeg.TryGetValue(key, out segmentId);
        }

        /// <summary>
        /// Removes a tile edge mapping.
        /// </summary>
        public bool Remove(uint key)
        {
            return _edgeToSeg.Remove(key);
        }

        /// <summary>
        /// Returns true when native storage is initialized.
        /// </summary>
        public readonly bool IsCreated => _edgeToSeg.IsCreated;

        /// <summary>
        /// Disposes native storage.
        /// </summary>
        public void Dispose()
        {
            if (_edgeToSeg.IsCreated)
            {
                _edgeToSeg.Dispose();
            }
        }
    }
}
