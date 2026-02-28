#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Logical transport lane semantics.
    /// </summary>
    public enum ProtocolLane : byte
    {
        ReliableOrdered = 0,
        UnreliableSequenced = 1
    }

    /// <summary>
    /// Protocol message identifiers for v1 envelope.
    /// </summary>
    public enum ProtocolMessageType : ushort
    {
        ServerHello = 2,
        ChunkSnapshotMeta = 9,
        ChunkSnapshotFrag = 10,
        ChunkPatchRect = 11,
        TrainStateUs = 30
    }

    /// <summary>
    /// Envelope metadata for protocol v1 framing.
    /// </summary>
    public struct ProtocolEnvelopeHeader
    {
        public const ushort Version = 1;

        public ushort ProtocolVersion;
        public ProtocolLane Lane;
        public ProtocolMessageType MessageType;
        public uint Sequence;
        public uint MessageId;
        public uint PayloadLength;
    }

    /// <summary>
    /// Canonical binary envelope read/write helpers with strict bounds checks.
    /// </summary>
    public static class ProtocolEnvelopeV1
    {
        public const int HeaderSize = 2 + 1 + 1 + 2 + 4 + 4 + 4;

        /// <summary>
        /// Writes envelope and payload into destination.
        /// </summary>
        public static bool TryWrite(
            ProtocolLane lane,
            ProtocolMessageType messageType,
            uint sequence,
            uint messageId,
            ReadOnlySpan<byte> payload,
            Span<byte> dst,
            out int bytesWritten)
        {
            bytesWritten = 0;
            int needed = HeaderSize + payload.Length;
            if (dst.Length < needed)
            {
                return false;
            }

            int o = 0;
            WriteU16(dst, ref o, ProtocolEnvelopeHeader.Version);
            dst[o++] = (byte)lane;
            dst[o++] = 0;
            WriteU16(dst, ref o, (ushort)messageType);
            WriteU32(dst, ref o, sequence);
            WriteU32(dst, ref o, messageId);
            WriteU32(dst, ref o, (uint)payload.Length);
            payload.CopyTo(dst.Slice(o, payload.Length));
            o += payload.Length;
            bytesWritten = o;
            return true;
        }

        /// <summary>
        /// Reads envelope header and payload span view.
        /// </summary>
        public static bool TryRead(ReadOnlySpan<byte> src, out ProtocolEnvelopeHeader header, out ReadOnlySpan<byte> payload)
        {
            header = default;
            payload = default;

            if (src.Length < HeaderSize)
            {
                return false;
            }

            int o = 0;
            header.ProtocolVersion = ReadU16(src, ref o);
            if (header.ProtocolVersion != ProtocolEnvelopeHeader.Version)
            {
                return false;
            }

            header.Lane = (ProtocolLane)src[o++];
            o++;
            header.MessageType = (ProtocolMessageType)ReadU16(src, ref o);
            header.Sequence = ReadU32(src, ref o);
            header.MessageId = ReadU32(src, ref o);
            header.PayloadLength = ReadU32(src, ref o);

            if (header.PayloadLength > (uint)(src.Length - HeaderSize))
            {
                return false;
            }

            payload = src.Slice(o, (int)header.PayloadLength);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteU16(Span<byte> dst, ref int o, ushort v)
        {
            dst[o + 0] = (byte)v;
            dst[o + 1] = (byte)(v >> 8);
            o += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort ReadU16(ReadOnlySpan<byte> src, ref int o)
        {
            ushort v = (ushort)(src[o + 0] | (src[o + 1] << 8));
            o += 2;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteU32(Span<byte> dst, ref int o, uint v)
        {
            dst[o + 0] = (byte)v;
            dst[o + 1] = (byte)(v >> 8);
            dst[o + 2] = (byte)(v >> 16);
            dst[o + 3] = (byte)(v >> 24);
            o += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ReadU32(ReadOnlySpan<byte> src, ref int o)
        {
            uint v = (uint)(src[o + 0] | (src[o + 1] << 8) | (src[o + 2] << 16) | (src[o + 3] << 24));
            o += 4;
            return v;
        }
    }
}
