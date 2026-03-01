using System.Runtime.CompilerServices;
using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Dense SoA store with stable <see cref="SegmentId" /> via id-to-dense mapping.
    /// Removal uses swap-remove while preserving external stable IDs.
    /// </summary>
    public struct SegmentStoreSoA : System.IDisposable
    {
        private NativeList<uint> _denseId;
        private NativeList<uint> _denseA;
        private NativeList<uint> _denseB;
        private NativeList<byte> _denseKind;
        private NativeList<byte> _denseFlags;
        private NativeList<ushort> _denseSpeed;

        private NativeParallelHashMap<uint, int> _idToDense;

        /// <summary>
        /// Creates the segment store with initial capacity.
        /// </summary>
        /// <param name="capacity">Initial storage capacity.</param>
        /// <param name="alloc">Allocator used for native containers.</param>
        /// <returns>Initialized segment store.</returns>
        public static SegmentStoreSoA Create(int capacity, Allocator alloc)
        {
            return new SegmentStoreSoA
            {
                _denseId = new NativeList<uint>(capacity, alloc),
                _denseA = new NativeList<uint>(capacity, alloc),
                _denseB = new NativeList<uint>(capacity, alloc),
                _denseKind = new NativeList<byte>(capacity, alloc),
                _denseFlags = new NativeList<byte>(capacity, alloc),
                _denseSpeed = new NativeList<ushort>(capacity, alloc),
                _idToDense = new NativeParallelHashMap<uint, int>(capacity * 2, alloc)
            };
        }

        /// <summary>
        /// Returns true when store is initialized.
        /// </summary>
        public readonly bool IsCreated => _denseId.IsCreated;

        /// <summary>
        /// Number of active dense segment entries.
        /// </summary>
        public readonly int Count => _denseId.Length;

        /// <summary>
        /// Tries to get dense index from stable segment id.
        /// </summary>
        /// <param name="id">Stable segment id.</param>
        /// <param name="denseIndex">Dense index when found.</param>
        /// <returns>True if found; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetDenseIndex(SegmentId id, out int denseIndex)
        {
            return _idToDense.TryGetValue(id.Value, out denseIndex);
        }

        /// <summary>
        /// Adds a segment to the dense store.
        /// </summary>
        /// <param name="id">Stable segment id.</param>
        /// <param name="a">Node A id.</param>
        /// <param name="b">Node B id.</param>
        /// <param name="kind">Segment kind.</param>
        /// <param name="flags">Segment flags.</param>
        /// <param name="speedClass">Speed/cost class.</param>
        /// <param name="denseIndex">Assigned dense index.</param>
        public void Add(
            SegmentId id,
            NodeId a,
            NodeId b,
            SegmentKind kind,
            SegmentFlags flags,
            ushort speedClass,
            out int denseIndex)
        {
            denseIndex = _denseId.Length;

            _denseId.Add(id.Value);
            _denseA.Add(a.Value);
            _denseB.Add(b.Value);
            _denseKind.Add((byte)kind);
            _denseFlags.Add((byte)flags);
            _denseSpeed.Add(speedClass);

            _idToDense.Add(id.Value, denseIndex);
        }

        /// <summary>
        /// Removes a segment using swap-remove and updates the id-to-dense map.
        /// </summary>
        /// <param name="id">Segment id to remove.</param>
        /// <param name="denseIndex">Current dense index of segment to remove.</param>
        public void RemoveSwap(SegmentId id, int denseIndex)
        {
            int last = _denseId.Length - 1;
            uint removedId = id.Value;

            if (denseIndex != last)
            {
                uint movedId = _denseId[last];

                _denseId[denseIndex] = _denseId[last];
                _denseA[denseIndex] = _denseA[last];
                _denseB[denseIndex] = _denseB[last];
                _denseKind[denseIndex] = _denseKind[last];
                _denseFlags[denseIndex] = _denseFlags[last];
                _denseSpeed[denseIndex] = _denseSpeed[last];

                _idToDense[movedId] = denseIndex;
            }

            _denseId.RemoveAt(last);
            _denseA.RemoveAt(last);
            _denseB.RemoveAt(last);
            _denseKind.RemoveAt(last);
            _denseFlags.RemoveAt(last);
            _denseSpeed.RemoveAt(last);

            _idToDense.Remove(removedId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint DenseId(int index)
        {
            return _denseId[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NodeId DenseA(int index)
        {
            return new NodeId(_denseA[index]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NodeId DenseB(int index)
        {
            return new NodeId(_denseB[index]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SegmentKind DenseKind(int index)
        {
            return (SegmentKind)_denseKind[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SegmentFlags DenseFlags(int index)
        {
            return (SegmentFlags)_denseFlags[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort DenseSpeed(int index)
        {
            return _denseSpeed[index];
        }

        /// <summary>
        /// Updates mutable metadata fields for a dense segment row.
        /// </summary>
        /// <param name="index">Dense row index.</param>
        /// <param name="flags">New segment flags.</param>
        /// <param name="speedClass">New speed class value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetDenseMetadata(int index, SegmentFlags flags, ushort speedClass)
        {
            _denseFlags[index] = (byte)flags;
            _denseSpeed[index] = speedClass;
        }

        /// <summary>
        /// Disposes all native containers owned by this store.
        /// </summary>
        public void Dispose()
        {
            if (_denseId.IsCreated)
            {
                _denseId.Dispose();
            }

            if (_denseA.IsCreated)
            {
                _denseA.Dispose();
            }

            if (_denseB.IsCreated)
            {
                _denseB.Dispose();
            }

            if (_denseKind.IsCreated)
            {
                _denseKind.Dispose();
            }

            if (_denseFlags.IsCreated)
            {
                _denseFlags.Dispose();
            }

            if (_denseSpeed.IsCreated)
            {
                _denseSpeed.Dispose();
            }

            if (_idToDense.IsCreated)
            {
                _idToDense.Dispose();
            }
        }
    }
}
