using System;
using System.Runtime.CompilerServices;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Stable authoritative identifier for a rail segment.
    /// </summary>
    public readonly struct SegmentId : IEquatable<SegmentId>
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

        public static implicit operator uint(SegmentId id)
        {
            return id.Value;
        }
    }

    /// <summary>
    /// Stable identifier for a rail topology node.
    /// </summary>
    public readonly struct NodeId : IEquatable<NodeId>
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

        public static implicit operator uint(NodeId id)
        {
            return id.Value;
        }
    }
}
