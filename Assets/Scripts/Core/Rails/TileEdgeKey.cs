using System.Runtime.CompilerServices;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Utilities for packing and unpacking canonical tile-edge keys.
    /// </summary>
    public static class TileEdgeKey
    {
        /// <summary>
        /// Canonical edge direction.
        /// </summary>
        public enum Dir : uint
        {
            Horizontal = 0,
            Vertical = 1
        }

        /// <summary>
        /// Packs x/y/dir into a 32-bit key using bits [0..10]=x, [11..21]=y, [22]=dir.
        /// </summary>
        /// <param name="x">Tile X coordinate (11-bit).</param>
        /// <param name="y">Tile Y coordinate (11-bit).</param>
        /// <param name="dir">Edge orientation.</param>
        /// <returns>Packed edge key.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Pack(uint x, uint y, Dir dir)
        {
            return (x & 0x7FFu) | ((y & 0x7FFu) << 11) | (((uint)dir & 1u) << 22);
        }

        /// <summary>
        /// Unpacks a tile-edge key into x/y/dir components.
        /// </summary>
        /// <param name="key">Packed edge key.</param>
        /// <param name="x">Unpacked tile X coordinate.</param>
        /// <param name="y">Unpacked tile Y coordinate.</param>
        /// <param name="dir">Unpacked edge orientation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unpack(uint key, out uint x, out uint y, out Dir dir)
        {
            x = key & 0x7FFu;
            y = (key >> 11) & 0x7FFu;
            dir = (Dir)((key >> 22) & 1u);
        }

        /// <summary>
        /// Creates a canonical unit-length edge key for axis-aligned straight segments.
        /// </summary>
        /// <param name="ax">Endpoint A tile X.</param>
        /// <param name="ay">Endpoint A tile Y.</param>
        /// <param name="bx">Endpoint B tile X.</param>
        /// <param name="by">Endpoint B tile Y.</param>
        /// <param name="key">Canonical packed edge key when successful.</param>
        /// <returns>True when the segment is exactly one tile long and axis-aligned; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFromUnitSegment(ushort ax, ushort ay, ushort bx, ushort by, out uint key)
        {
            int dx = bx - ax;
            int dy = by - ay;

            if (dy == 0 && (dx == 1 || dx == -1))
            {
                ushort x = (ushort)(ax < bx ? ax : bx);
                key = Pack(x, ay, Dir.Horizontal);
                return true;
            }

            if (dx == 0 && (dy == 1 || dy == -1))
            {
                ushort y = (ushort)(ay < by ? ay : by);
                key = Pack(ax, y, Dir.Vertical);
                return true;
            }

            key = 0;
            return false;
        }
    }
}
