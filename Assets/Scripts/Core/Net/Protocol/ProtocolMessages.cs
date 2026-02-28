#nullable enable
using System;
using System.Runtime.CompilerServices;
using OpenTTD.Core.World;
using OpenTTD.Core.WorldGen;

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Message payload codecs for protocol envelope v1.
    /// Each payload begins with a payload version byte.
    /// </summary>
    public static class ProtocolMessages
    {
        public const byte ServerHelloPayloadVer = 1;
        public const byte ChunkSnapshotFragPayloadVer = 1;
        public const byte ResyncChunkRequestPayloadVer = 1;

        /// <summary>
        /// Writes ServerHello payload bytes.
        /// </summary>
        public static int WriteServerHello(in WorldGenConfig cfg, uint serverTickRateHz, Span<byte> dst, ulong configCrc64)
        {
            int cfgLen = WorldGenConfigBlob.SizeBytes;
            int needed = 1 + 4 + 8 + 2 + 2 + 2 + 1 + 1 + 4 + 2 + cfgLen + 8;
            if (dst.Length < needed)
            {
                throw new ArgumentOutOfRangeException(nameof(dst));
            }

            int o = 0;
            dst[o++] = ServerHelloPayloadVer;

            WriteU32(dst, ref o, cfg.WorldGenVersion);
            WriteU64(dst, ref o, cfg.WorldSeed);

            WriteU16(dst, ref o, (ushort)WorldConstants.MapW);
            WriteU16(dst, ref o, (ushort)WorldConstants.MapH);
            WriteU16(dst, ref o, (ushort)WorldConstants.ChunkSize);

            dst[o++] = cfg.SeaLevel;
            dst[o++] = 0;

            WriteU32(dst, ref o, serverTickRateHz);

            WriteU16(dst, ref o, (ushort)cfgLen);
            WorldGenConfigBlob.Write(cfg, dst.Slice(o, cfgLen));
            o += cfgLen;

            WriteU64(dst, ref o, configCrc64);
            return o;
        }

        /// <summary>
        /// Reads ServerHello payload bytes.
        /// </summary>
        public static bool TryReadServerHello(ReadOnlySpan<byte> src, out WorldGenConfig cfg, out uint serverTickRateHz, out ulong configCrc64)
        {
            cfg = default;
            serverTickRateHz = 0;
            configCrc64 = 0;

            if (src.Length < 1)
            {
                return false;
            }

            int o = 0;
            byte ver = src[o++];
            if (ver != ServerHelloPayloadVer)
            {
                return false;
            }

            if (src.Length < o + 4 + 8 + 2 + 2 + 2 + 1 + 1 + 4 + 2)
            {
                return false;
            }

            uint worldGenVersion = ReadU32(src, ref o);
            ulong worldSeed = ReadU64(src, ref o);

            ushort mapW = ReadU16(src, ref o);
            ushort mapH = ReadU16(src, ref o);
            ushort chunkSize = ReadU16(src, ref o);

            byte sea = src[o++];
            o++;

            serverTickRateHz = ReadU32(src, ref o);

            ushort cfgLen = ReadU16(src, ref o);
            if (src.Length < o + cfgLen + 8)
            {
                return false;
            }

            cfg = WorldGenConfigBlob.Read(src.Slice(o, cfgLen));
            o += cfgLen;

            configCrc64 = ReadU64(src, ref o);

            if (cfg.WorldGenVersion != worldGenVersion)
            {
                return false;
            }

            if (cfg.WorldSeed != worldSeed)
            {
                return false;
            }

            if (cfg.SeaLevel != sea)
            {
                return false;
            }

            if (mapW != WorldConstants.MapW)
            {
                return false;
            }

            if (mapH != WorldConstants.MapH)
            {
                return false;
            }

            if (chunkSize != WorldConstants.ChunkSize)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Writes ChunkSnapshotFrag payload bytes.
        /// </summary>
        public static int WriteChunkSnapshotFrag(
            short cx,
            short cy,
            uint snapshotId,
            uint totalLen,
            ushort fragIndex,
            ushort fragCount,
            ushort codec,
            ReadOnlySpan<byte> fragPayload,
            Span<byte> dst)
        {
            int needed = 1 + 2 + 2 + 4 + 4 + 2 + 2 + 2 + 2 + fragPayload.Length;
            if (dst.Length < needed)
            {
                throw new ArgumentOutOfRangeException(nameof(dst));
            }

            int o = 0;
            dst[o++] = ChunkSnapshotFragPayloadVer;

            WriteI16(dst, ref o, cx);
            WriteI16(dst, ref o, cy);
            WriteU32(dst, ref o, snapshotId);
            WriteU32(dst, ref o, totalLen);

            WriteU16(dst, ref o, fragIndex);
            WriteU16(dst, ref o, fragCount);
            WriteU16(dst, ref o, (ushort)fragPayload.Length);
            WriteU16(dst, ref o, codec);

            fragPayload.CopyTo(dst.Slice(o));
            o += fragPayload.Length;
            return o;
        }

        /// <summary>
        /// Reads ChunkSnapshotFrag payload bytes.
        /// </summary>
        public static bool TryReadChunkSnapshotFrag(
            ReadOnlySpan<byte> src,
            out short cx,
            out short cy,
            out uint snapshotId,
            out uint totalLen,
            out ushort fragIndex,
            out ushort fragCount,
            out ushort fragLen,
            out ushort codec,
            out ReadOnlySpan<byte> fragPayload)
        {
            cx = 0;
            cy = 0;
            snapshotId = 0;
            totalLen = 0;
            fragIndex = 0;
            fragCount = 0;
            fragLen = 0;
            codec = 0;
            fragPayload = default;

            if (src.Length < 1)
            {
                return false;
            }

            int o = 0;
            byte ver = src[o++];
            if (ver != ChunkSnapshotFragPayloadVer)
            {
                return false;
            }

            if (src.Length < o + 2 + 2 + 4 + 4 + 2 + 2 + 2 + 2)
            {
                return false;
            }

            cx = ReadI16(src, ref o);
            cy = ReadI16(src, ref o);
            snapshotId = ReadU32(src, ref o);
            totalLen = ReadU32(src, ref o);

            fragIndex = ReadU16(src, ref o);
            fragCount = ReadU16(src, ref o);
            fragLen = ReadU16(src, ref o);
            codec = ReadU16(src, ref o);

            if (fragIndex >= fragCount)
            {
                return false;
            }

            if (src.Length < o + fragLen)
            {
                return false;
            }

            fragPayload = src.Slice(o, fragLen);
            return true;
        }

        /// <summary>
        /// Writes ResyncChunkRequest payload bytes.
        /// </summary>
        public static int WriteResyncChunkRequest(short cx, short cy, uint clientSnapshotId, Span<byte> dst)
        {
            int needed = 1 + 2 + 2 + 4;
            if (dst.Length < needed)
            {
                throw new ArgumentOutOfRangeException(nameof(dst));
            }

            int o = 0;
            dst[o++] = ResyncChunkRequestPayloadVer;
            WriteI16(dst, ref o, cx);
            WriteI16(dst, ref o, cy);
            WriteU32(dst, ref o, clientSnapshotId);
            return o;
        }

        /// <summary>
        /// Reads ResyncChunkRequest payload bytes.
        /// </summary>
        public static bool TryReadResyncChunkRequest(ReadOnlySpan<byte> src, out short cx, out short cy, out uint clientSnapshotId)
        {
            cx = 0;
            cy = 0;
            clientSnapshotId = 0;

            if (src.Length < 1)
            {
                return false;
            }

            int o = 0;
            byte ver = src[o++];
            if (ver != ResyncChunkRequestPayloadVer)
            {
                return false;
            }

            if (src.Length < o + 2 + 2 + 4)
            {
                return false;
            }

            cx = ReadI16(src, ref o);
            cy = ReadI16(src, ref o);
            clientSnapshotId = ReadU32(src, ref o);
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
        private static void WriteI16(Span<byte> dst, ref int o, short v)
        {
            WriteU16(dst, ref o, unchecked((ushort)v));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static short ReadI16(ReadOnlySpan<byte> src, ref int o)
        {
            return unchecked((short)ReadU16(src, ref o));
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteU64(Span<byte> dst, ref int o, ulong v)
        {
            dst[o + 0] = (byte)v;
            dst[o + 1] = (byte)(v >> 8);
            dst[o + 2] = (byte)(v >> 16);
            dst[o + 3] = (byte)(v >> 24);
            dst[o + 4] = (byte)(v >> 32);
            dst[o + 5] = (byte)(v >> 40);
            dst[o + 6] = (byte)(v >> 48);
            dst[o + 7] = (byte)(v >> 56);
            o += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ReadU64(ReadOnlySpan<byte> src, ref int o)
        {
            ulong v =
                ((ulong)src[o + 0])
                | ((ulong)src[o + 1] << 8)
                | ((ulong)src[o + 2] << 16)
                | ((ulong)src[o + 3] << 24)
                | ((ulong)src[o + 4] << 32)
                | ((ulong)src[o + 5] << 40)
                | ((ulong)src[o + 6] << 48)
                | ((ulong)src[o + 7] << 56);
            o += 8;
            return v;
        }
    }
}
