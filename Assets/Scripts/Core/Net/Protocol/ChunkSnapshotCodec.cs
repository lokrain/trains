#nullable enable
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using OpenTTD.Core.World;

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Chunk snapshot encoding/decoding helpers.
    /// Core payload layout is stable; compression implementation is provided via <see cref="ICompressor"/>.
    /// </summary>
    public static class ChunkSnapshotCodec
    {
        /// <summary>
        /// Snapshot payload field mask.
        /// </summary>
        [Flags]
        public enum FieldsMask : byte
        {
            Height = 1 << 0,
            RiverMask = 1 << 1,
            Biome = 1 << 2
        }

        /// <summary>
        /// Snapshot payload header.
        /// </summary>
        public struct Header
        {
            public uint SnapshotId;
            public byte PayloadVer;
            public FieldsMask Mask;
            public ushort Reserved;
        }

        public const byte PayloadVersion = 1;

        /// <summary>
        /// Computes the max uncompressed payload size for a given mask.
        /// </summary>
        public static int GetMaxDecompressedSize(FieldsMask mask)
        {
            int n = 8;
            if ((mask & FieldsMask.Height) != 0)
            {
                n += 4096;
            }

            if ((mask & FieldsMask.RiverMask) != 0)
            {
                n += 4096;
            }

            if ((mask & FieldsMask.Biome) != 0)
            {
                n += 4096;
            }

            return n;
        }

        /// <summary>
        /// Encodes a chunk snapshot to compressed bytes.
        /// Caller handles transport fragmentation.
        /// </summary>
        public static int EncodeCompressed(
            ref ChunkSoA chunk,
            uint snapshotId,
            FieldsMask mask,
            ICompressor compressor,
            Span<byte> compressedDst,
            out int uncompressedLen)
        {
            uncompressedLen = GetMaxDecompressedSize(mask);

            byte[] tmp = ArrayPool<byte>.Shared.Rent(uncompressedLen);
            try
            {
                Span<byte> buf = tmp.AsSpan(0, uncompressedLen);
                WriteHeader(buf, snapshotId, mask);

                int o = 8;
                if ((mask & FieldsMask.Height) != 0)
                {
                    CopyNativeToSpan(chunk.Height, buf.Slice(o, 4096));
                    o += 4096;
                }

                if ((mask & FieldsMask.RiverMask) != 0)
                {
                    CopyNativeToSpan(chunk.RiverMask, buf.Slice(o, 4096));
                    o += 4096;
                }

                if ((mask & FieldsMask.Biome) != 0)
                {
                    CopyNativeToSpan(chunk.Biome, buf.Slice(o, 4096));
                    o += 4096;
                }

                uncompressedLen = o;
                return compressor.Compress(buf.Slice(0, uncompressedLen), compressedDst);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(tmp);
            }
        }

        /// <summary>
        /// Decodes compressed snapshot bytes into provided scratch buffer.
        /// </summary>
        public static bool DecodeCompressed(
            ReadOnlySpan<byte> compressedSrc,
            ICompressor compressor,
            Span<byte> decompressedScratch,
            out Header header,
            out ReadOnlySpan<byte> payload)
        {
            header = default;
            payload = default;

            int written = compressor.Decompress(compressedSrc, decompressedScratch);
            if (written < 8)
            {
                return false;
            }

            header = ReadHeader(decompressedScratch.Slice(0, 8));
            if (header.PayloadVer != PayloadVersion)
            {
                return false;
            }

            payload = decompressedScratch.Slice(8, written - 8);
            return true;
        }

        /// <summary>
        /// Applies decoded payload fields into chunk SoA arrays.
        /// </summary>
        public static bool ApplyDecodedPayloadToChunk(
            in Header header,
            ReadOnlySpan<byte> payload,
            ref ChunkSoA chunk)
        {
            int o = 0;

            if ((header.Mask & FieldsMask.Height) != 0)
            {
                if (payload.Length < o + 4096)
                {
                    return false;
                }

                CopySpanToNative(payload.Slice(o, 4096), chunk.Height);
                o += 4096;
            }

            if ((header.Mask & FieldsMask.RiverMask) != 0)
            {
                if (payload.Length < o + 4096)
                {
                    return false;
                }

                CopySpanToNative(payload.Slice(o, 4096), chunk.RiverMask);
                o += 4096;
            }

            if ((header.Mask & FieldsMask.Biome) != 0)
            {
                if (payload.Length < o + 4096)
                {
                    return false;
                }

                CopySpanToNative(payload.Slice(o, 4096), chunk.Biome);
                o += 4096;
            }

            chunk.Versions.SnapshotVersion = header.SnapshotId;
            chunk.Versions.HeightVersion = header.SnapshotId;
            chunk.Dirty |= ChunkDirtyFlags.Derived;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteHeader(Span<byte> dst, uint snapshotId, FieldsMask mask)
        {
            WriteU32LE(dst, 0, snapshotId);
            dst[4] = PayloadVersion;
            dst[5] = (byte)mask;
            dst[6] = 0;
            dst[7] = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Header ReadHeader(ReadOnlySpan<byte> src)
        {
            return new Header
            {
                SnapshotId = ReadU32LE(src, 0),
                PayloadVer = src[4],
                Mask = (FieldsMask)src[5],
                Reserved = 0
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteU32LE(Span<byte> dst, int offset, uint v)
        {
            dst[offset + 0] = (byte)v;
            dst[offset + 1] = (byte)(v >> 8);
            dst[offset + 2] = (byte)(v >> 16);
            dst[offset + 3] = (byte)(v >> 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ReadU32LE(ReadOnlySpan<byte> src, int offset)
        {
            return (uint)(src[offset + 0]
                | (src[offset + 1] << 8)
                | (src[offset + 2] << 16)
                | (src[offset + 3] << 24));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CopyNativeToSpan(Unity.Collections.NativeArray<byte> src, Span<byte> dst)
        {
            for (int i = 0; i < dst.Length; i++)
            {
                dst[i] = src[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CopySpanToNative(ReadOnlySpan<byte> src, Unity.Collections.NativeArray<byte> dst)
        {
            for (int i = 0; i < src.Length; i++)
            {
                dst[i] = src[i];
            }
        }
    }
}
