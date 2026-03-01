using System.Runtime.CompilerServices;
using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Stable NodeId to packed tile-center coordinate and adjacency head mapping.
    /// </summary>
    public struct NodeTable : System.IDisposable
    {
        private NativeList<uint> _posPacked;
        private NativeList<int> _headEdge;
        private NativeParallelHashMap<uint, uint> _posToNode;

        /// <summary>
        /// Creates a node table with initial capacity.
        /// </summary>
        /// <param name="capacity">Initial node capacity.</param>
        /// <param name="alloc">Allocator type for native containers.</param>
        /// <returns>Initialized node table.</returns>
        public static NodeTable Create(int capacity, Allocator alloc)
        {
            return new NodeTable
            {
                _posPacked = new NativeList<uint>(capacity, alloc),
                _headEdge = new NativeList<int>(capacity, alloc),
                _posToNode = new NativeParallelHashMap<uint, uint>(capacity * 2, alloc)
            };
        }

        /// <summary>
        /// Returns true when all backing native containers are initialized.
        /// </summary>
        public readonly bool IsCreated => _posPacked.IsCreated;

        /// <summary>
        /// Packs tile center position into 11-bit x and 11-bit y format.
        /// </summary>
        /// <param name="x">Tile X coordinate.</param>
        /// <param name="y">Tile Y coordinate.</param>
        /// <returns>Packed position key.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PackPos(ushort x, ushort y)
        {
            return (uint)(x & 0x7FF) | ((uint)(y & 0x7FF) << 11);
        }

        /// <summary>
        /// Ensures backing arrays can index at least up to the given size.
        /// </summary>
        /// <param name="size">Minimum required length.</param>
        public void EnsureCapacity(int size)
        {
            while (_posPacked.Length < size)
            {
                _posPacked.Add(0);
            }

            while (_headEdge.Length < size)
            {
                _headEdge.Add(-1);
            }
        }

        /// <summary>
        /// Tries to resolve a packed position to an existing node id.
        /// </summary>
        /// <param name="packedPos">Packed tile center position.</param>
        /// <param name="id">Resolved node id when found.</param>
        /// <returns>True when a node exists at the packed position.</returns>
        public bool TryGetNode(uint packedPos, out NodeId id)
        {
            if (_posToNode.TryGetValue(packedPos, out uint value))
            {
                id = new NodeId(value);
                return true;
            }

            id = NodeId.Null;
            return false;
        }

        /// <summary>
        /// Inserts or updates node mapping for a packed position.
        /// </summary>
        /// <param name="id">Node identifier.</param>
        /// <param name="packedPos">Packed tile center position.</param>
        public void InsertNode(NodeId id, uint packedPos)
        {
            EnsureCapacity((int)id.Value + 1);
            _posPacked[(int)id.Value] = packedPos;
            _headEdge[(int)id.Value] = -1;
            _posToNode[packedPos] = id.Value;
        }

        /// <summary>
        /// Removes node position mapping when the node has no attached adjacency.
        /// </summary>
        /// <param name="id">Node identifier.</param>
        public void RemoveNodeMapping(NodeId id)
        {
            int index = (int)id.Value;
            if (index <= 0 || index >= _posPacked.Length)
            {
                return;
            }

            uint packed = _posPacked[index];
            if (packed != 0)
            {
                _posToNode.Remove(packed);
            }

            _posPacked[index] = 0;
            _headEdge[index] = -1;
        }

        /// <summary>
        /// Gets adjacency-list head edge index for a node.
        /// </summary>
        /// <param name="id">Node identifier.</param>
        /// <returns>Edge head index or -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHeadEdge(NodeId id)
        {
            return _headEdge[(int)id.Value];
        }

        /// <summary>
        /// Sets adjacency-list head edge index for a node.
        /// </summary>
        /// <param name="id">Node identifier.</param>
        /// <param name="head">New edge head index.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetHeadEdge(NodeId id, int head)
        {
            _headEdge[(int)id.Value] = head;
        }

        /// <summary>
        /// Returns the highest usable node id index currently backed by table storage.
        /// </summary>
        public readonly int MaxNodeIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _headEdge.Length - 1;
            }
        }

        /// <summary>
        /// Tries to retrieve unpacked tile center coordinates for a node.
        /// </summary>
        /// <param name="id">Node identifier.</param>
        /// <param name="x">Unpacked tile X coordinate.</param>
        /// <param name="y">Unpacked tile Y coordinate.</param>
        /// <returns>True when node id is valid and position exists.</returns>
        public bool TryGetPos(NodeId id, out ushort x, out ushort y)
        {
            int index = (int)id.Value;
            if ((uint)index >= (uint)_posPacked.Length || id.Value == 0)
            {
                x = 0;
                y = 0;
                return false;
            }

            uint packed = _posPacked[index];
            x = (ushort)(packed & 0x7FFu);
            y = (ushort)((packed >> 11) & 0x7FFu);
            return true;
        }

        /// <summary>
        /// Disposes all native containers owned by the node table.
        /// </summary>
        public void Dispose()
        {
            if (_posPacked.IsCreated)
            {
                _posPacked.Dispose();
            }

            if (_headEdge.IsCreated)
            {
                _headEdge.Dispose();
            }

            if (_posToNode.IsCreated)
            {
                _posToNode.Dispose();
            }
        }
    }
}
