using System.Runtime.CompilerServices;
using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Deterministic allocator for stable segment identifiers with free-list reuse.
    /// Tracks active IDs to prevent duplicate active allocations.
    /// </summary>
    public struct SegmentIdAllocator : System.IDisposable
    {
        private uint _next;
        private NativeQueue<uint> _free;
        private NativeParallelHashSet<uint> _active;

        public static SegmentIdAllocator Create(uint startInclusive, Allocator alloc, int expectedCapacity = 4096)
        {
            return new SegmentIdAllocator
            {
                _next = startInclusive == 0 ? 1u : startInclusive,
                _free = new NativeQueue<uint>(alloc),
                _active = new NativeParallelHashSet<uint>(expectedCapacity, alloc)
            };
        }

        public readonly bool IsCreated => _free.IsCreated && _active.IsCreated;

        public readonly int ActiveCount => _active.Count();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SegmentId Alloc()
        {
            uint id;
            do
            {
                id = _free.Count > 0 ? _free.Dequeue() : _next++;
            }
            while (id == 0 || _active.Contains(id));

            _active.Add(id);
            return new SegmentId(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Free(SegmentId id)
        {
            uint raw = id.Value;
            if (raw == 0)
            {
                return;
            }

            if (_active.Remove(raw))
            {
                _free.Enqueue(raw);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsActive(SegmentId id)
        {
            return id.Value != 0 && _active.Contains(id.Value);
        }

        public void Dispose()
        {
            if (_free.IsCreated)
            {
                _free.Dispose();
            }

            if (_active.IsCreated)
            {
                _active.Dispose();
            }
        }
    }
}
