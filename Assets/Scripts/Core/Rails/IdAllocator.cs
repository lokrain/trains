using System.Runtime.CompilerServices;
using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Deterministic server-owned allocator: stable IDs plus deterministic reuse via FIFO free-list.
    /// </summary>
    public struct IdAllocator : System.IDisposable
    {
        private uint _next;
        private NativeQueue<uint> _free;

        /// <summary>
        /// Creates an allocator with a starting inclusive ID and allocator type.
        /// </summary>
        /// <param name="startInclusive">First ID to allocate when free-list is empty.</param>
        /// <param name="alloc">Native allocation strategy.</param>
        /// <returns>Initialized allocator instance.</returns>
        public static IdAllocator Create(uint startInclusive, Allocator alloc)
        {
            return new IdAllocator
            {
                _next = startInclusive,
                _free = new NativeQueue<uint>(alloc)
            };
        }

        /// <summary>
        /// Returns true when internal native storage is initialized.
        /// </summary>
        public readonly bool IsCreated => _free.IsCreated;

        /// <summary>
        /// Allocates a stable identifier.
        /// </summary>
        /// <returns>Allocated ID.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Alloc()
        {
            return _free.Count > 0 ? _free.Dequeue() : _next++;
        }

        /// <summary>
        /// Returns an ID to the deterministic free-list for reuse.
        /// </summary>
        /// <param name="id">ID to free. Zero is ignored as reserved null sentinel.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Free(uint id)
        {
            if (id != 0)
            {
                _free.Enqueue(id);
            }
        }

        /// <summary>
        /// Disposes native storage.
        /// </summary>
        public void Dispose()
        {
            if (_free.IsCreated)
            {
                _free.Dispose();
            }
        }
    }
}
