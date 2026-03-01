using System;
using System.Runtime.CompilerServices;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Stable authoritative identifier for a rail segment.
    /// </summary>
    public readonly struct SegmentId : IEquatable<SegmentId>, IComparable<SegmentId>
    {
        /// <summary>
        /// Raw identifier value.
        /// </summary>
        public readonly uint Value;

        /// <summary>
        /// Creates a new segment identifier.
        /// </summary>
        /// <param name="value">Raw identifier value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SegmentId(uint value)
        {
            Value = value;
        }

        /// <summary>
        /// Represents an uninitialized segment identifier.
        /// </summary>
        public static SegmentId Null => new(0);

        public bool IsValid => Value != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(SegmentId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is SegmentId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(SegmentId left, SegmentId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SegmentId left, SegmentId right)
        {
            return !left.Equals(right);
        }

        public int CompareTo(SegmentId other)
        {
            return Value.CompareTo(other.Value);
        }

        public static implicit operator uint(SegmentId id)
        {
            return id.Value;
        }
    }

    /// <summary>
    /// Stable identifier for a rail topology node.
    /// </summary>
    public readonly struct NodeId : IEquatable<NodeId>, IComparable<NodeId>
    {
        /// <summary>
        /// Raw identifier value.
        /// </summary>
        public readonly uint Value;

        /// <summary>
        /// Creates a new node identifier.
        /// </summary>
        /// <param name="value">Raw identifier value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NodeId(uint value)
        {
            Value = value;
        }

        /// <summary>
        /// Represents an uninitialized node identifier.
        /// </summary>
        public static NodeId Null => new(0);

        public bool IsValid => Value != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(NodeId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is NodeId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(NodeId left, NodeId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NodeId left, NodeId right)
        {
            return !left.Equals(right);
        }

        public int CompareTo(NodeId other)
        {
            return Value.CompareTo(other.Value);
        }

        public static implicit operator uint(NodeId id)
        {
            return id.Value;
        }
    }

    /// <summary>
    /// Stable identifier for adjacency edge records.
    /// </summary>
    public readonly struct EdgeId : IEquatable<EdgeId>, IComparable<EdgeId>
    {
        /// <summary>
        /// Raw identifier value.
        /// </summary>
        public readonly uint Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EdgeId(uint value)
        {
            Value = value;
        }

        public static EdgeId Null => new(0);

        public bool IsValid => Value != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(EdgeId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is EdgeId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(EdgeId left, EdgeId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EdgeId left, EdgeId right)
        {
            return !left.Equals(right);
        }

        public int CompareTo(EdgeId other)
        {
            return Value.CompareTo(other.Value);
        }

        public static implicit operator uint(EdgeId id)
        {
            return id.Value;
        }
    }

    /// <summary>
    /// Little-endian serialization helpers for rail id primitives.
    /// </summary>
    public static class RailIdCodec
    {
        /// <summary>
        /// Writes SegmentId, NodeId, and EdgeId (12 bytes total) to little-endian payload.
        /// </summary>
        public static bool TryWrite(SegmentId segmentId, NodeId nodeId, EdgeId edgeId, Span<byte> dst)
        {
            if (dst.Length < 12)
            {
                return false;
            }

            WriteU32LE(dst, 0, segmentId.Value);
            WriteU32LE(dst, 4, nodeId.Value);
            WriteU32LE(dst, 8, edgeId.Value);
            return true;
        }

        /// <summary>
        /// Reads SegmentId, NodeId, and EdgeId (12 bytes total) from little-endian payload.
        /// </summary>
        public static bool TryRead(ReadOnlySpan<byte> src, out SegmentId segmentId, out NodeId nodeId, out EdgeId edgeId)
        {
            if (src.Length < 12)
            {
                segmentId = SegmentId.Null;
                nodeId = NodeId.Null;
                edgeId = EdgeId.Null;
                return false;
            }

            segmentId = new SegmentId(ReadU32LE(src, 0));
            nodeId = new NodeId(ReadU32LE(src, 4));
            edgeId = new EdgeId(ReadU32LE(src, 8));
            return true;
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
