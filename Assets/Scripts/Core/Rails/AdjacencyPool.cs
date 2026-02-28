using System.Runtime.CompilerServices;
using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Pooled adjacency records (edge list per node).
    /// Supports O(1) push-front insert and O(degree) removal via list traversal.
    /// </summary>
    public struct AdjacencyPool : System.IDisposable
    {
        /// <summary>
        /// Edge record used by node adjacency lists.
        /// </summary>
        public struct EdgeRec
        {
            /// <summary>
            /// Next edge index in the adjacency linked list.
            /// </summary>
            public int Next;

            /// <summary>
            /// Stable segment id connected by this edge.
            /// </summary>
            public SegmentId SegmentId;
        }

        private NativeList<EdgeRec> _pool;
        private int _freeHead;
        private Allocator _allocator;

        /// <summary>
        /// Creates a new adjacency pool.
        /// </summary>
        /// <param name="capacity">Initial pool capacity.</param>
        /// <param name="alloc">Native allocator type.</param>
        /// <returns>Initialized adjacency pool.</returns>
        public static AdjacencyPool Create(int capacity, Allocator alloc)
        {
            return new AdjacencyPool
            {
                _pool = new NativeList<EdgeRec>(capacity, alloc),
                _freeHead = -1,
                _allocator = alloc
            };
        }

        /// <summary>
        /// Returns true when internal pool storage is initialized.
        /// </summary>
        public readonly bool IsCreated => _pool.IsCreated;

        /// <summary>
        /// Allocates an edge record and returns its index.
        /// </summary>
        /// <param name="segId">Segment id to store in record.</param>
        /// <param name="next">Next edge index in linked list.</param>
        /// <returns>Allocated edge record index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Alloc(SegmentId segId, int next)
        {
            if (_freeHead != -1)
            {
                int index = _freeHead;
                EdgeRec rec = _pool[index];
                _freeHead = rec.Next;
                rec.Next = next;
                rec.SegmentId = segId;
                _pool[index] = rec;
                return index;
            }

            _pool.Add(new EdgeRec
            {
                SegmentId = segId,
                Next = next
            });
            return _pool.Length - 1;
        }

        /// <summary>
        /// Frees an edge record index back to the internal free list.
        /// </summary>
        /// <param name="idx">Edge record index to free.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Free(int idx)
        {
            EdgeRec rec = _pool[idx];
            rec.SegmentId = SegmentId.Null;
            rec.Next = _freeHead;
            _pool[idx] = rec;
            _freeHead = idx;
        }

        /// <summary>
        /// Returns a mutable reference to an edge record by index.
        /// </summary>
        /// <param name="idx">Edge record index.</param>
        /// <returns>Reference to edge record.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref EdgeRec ElementAt(int idx)
        {
            return ref _pool.ElementAt(idx);
        }

        /// <summary>
        /// Exposes the underlying pool list.
        /// </summary>
        public readonly NativeList<EdgeRec> Pool => _pool;

        /// <summary>
        /// Current free-list head index.
        /// </summary>
        public readonly int FreeHead => _freeHead;

        /// <summary>
        /// Allocator used to create this pool.
        /// </summary>
        public readonly Allocator Allocator => _allocator;

        /// <summary>
        /// Disposes native pool storage.
        /// </summary>
        public void Dispose()
        {
            if (_pool.IsCreated)
            {
                _pool.Dispose();
            }
        }
    }
}
