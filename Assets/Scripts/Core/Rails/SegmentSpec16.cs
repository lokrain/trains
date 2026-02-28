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
    }
}
