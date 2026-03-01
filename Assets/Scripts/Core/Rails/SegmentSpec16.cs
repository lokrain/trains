using System;
using System.Runtime.InteropServices;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Rail segment semantic type.
    /// </summary>
    public enum SegmentKind : byte
    {
        Straight = 0,
        Bridge = 1,
        Tunnel = 2
    }

    /// <summary>
    /// Optional bit flags for rail segment metadata.
    /// </summary>
    [Flags]
    public enum SegmentFlags : byte
    {
        None = 0
    }

    /// <summary>
    /// SIMD-friendly wire/event record. Exactly 16 bytes.
    /// Little-endian on the wire; in-memory layout is sequential.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
    public struct SegmentSpec16
    {
    /// <summary>
    /// Exact serialized wire size in bytes.
    /// </summary>
    public const int WireSize = 16;

        /// <summary>
        /// Segment endpoint A tile coordinate X.
        /// </summary>
        public ushort Ax;

        /// <summary>
        /// Segment endpoint A tile coordinate Y.
        /// </summary>
        public ushort Ay;

        /// <summary>
        /// Segment endpoint B tile coordinate X.
        /// </summary>
        public ushort Bx;

        /// <summary>
        /// Segment endpoint B tile coordinate Y.
        /// </summary>
        public ushort By;

        /// <summary>
        /// Segment shape/type classification.
        /// </summary>
        public SegmentKind Kind;

        /// <summary>
        /// Segment metadata flags.
        /// </summary>
        public SegmentFlags Flags;

        /// <summary>
        /// Speed/cost class.
        /// </summary>
        public ushort SpeedClass;

        /// <summary>
        /// Stable authoritative segment identifier from server.
        /// </summary>
        public SegmentId SegmentId;

    /// <summary>
    /// Writes this segment spec to little-endian wire payload.
    /// </summary>
    /// <param name="dst">Destination span. Must be at least 16 bytes.</param>
    /// <returns>True on success; false when destination is too small.</returns>
    public readonly bool TryWriteLittleEndian(Span<byte> dst)
    {
        if (dst.Length < WireSize)
        {
            return false;
        }

        WriteU16LE(dst, 0, Ax);
        WriteU16LE(dst, 2, Ay);
        WriteU16LE(dst, 4, Bx);
        WriteU16LE(dst, 6, By);
        dst[8] = (byte)Kind;
        dst[9] = (byte)Flags;
        WriteU16LE(dst, 10, SpeedClass);
        WriteU32LE(dst, 12, SegmentId.Value);
        return true;
    }

    /// <summary>
    /// Reads a segment spec from little-endian wire payload.
    /// </summary>
    /// <param name="src">Source span. Must be at least 16 bytes.</param>
    /// <param name="value">Decoded value on success.</param>
    /// <returns>True on success; false when source is too small.</returns>
    public static bool TryReadLittleEndian(ReadOnlySpan<byte> src, out SegmentSpec16 value)
    {
        if (src.Length < WireSize)
        {
            value = default;
            return false;
        }

        value = new SegmentSpec16
        {
            Ax = ReadU16LE(src, 0),
            Ay = ReadU16LE(src, 2),
            Bx = ReadU16LE(src, 4),
            By = ReadU16LE(src, 6),
            Kind = (SegmentKind)src[8],
            Flags = (SegmentFlags)src[9],
            SpeedClass = ReadU16LE(src, 10),
            SegmentId = new SegmentId(ReadU32LE(src, 12))
        };

        return true;
    }

    private static void WriteU16LE(Span<byte> dst, int offset, ushort value)
    {
        dst[offset + 0] = (byte)value;
        dst[offset + 1] = (byte)(value >> 8);
    }

    private static ushort ReadU16LE(ReadOnlySpan<byte> src, int offset)
    {
        return (ushort)(src[offset + 0] | (src[offset + 1] << 8));
    }

    private static void WriteU32LE(Span<byte> dst, int offset, uint value)
    {
        dst[offset + 0] = (byte)value;
        dst[offset + 1] = (byte)(value >> 8);
        dst[offset + 2] = (byte)(value >> 16);
        dst[offset + 3] = (byte)(value >> 24);
    }

    private static uint ReadU32LE(ReadOnlySpan<byte> src, int offset)
    {
        return (uint)(src[offset + 0]
            | (src[offset + 1] << 8)
            | (src[offset + 2] << 16)
            | (src[offset + 3] << 24));
    }
    }
}
