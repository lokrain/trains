#nullable enable
using System;
using System.Collections.Generic;
using OpenTTD.Core.Net.Protocol;
using OpenTTD.Core.World;

namespace OpenTTD.Core.Client.Net
{
    /// <summary>
    /// Receives chunk snapshot fragments and applies decoded snapshots to client world cache.
    /// totalLen is parsed from fragment body payload.
    /// </summary>
    public sealed class WorldSnapshotReceiver : IDisposable
    {
        private readonly ICompressor _compressor;
        private readonly int _fragPayloadCap;

        private readonly Dictionary<ulong, ReassemblyBuffer> _reassembly = new Dictionary<ulong, ReassemblyBuffer>(256);
        private readonly byte[] _decompressedScratch = new byte[8 + 4096 * 3];

        public WorldSnapshotReceiver(ICompressor compressor, int fragPayloadCap)
        {
            _compressor = compressor;
            _fragPayloadCap = fragPayloadCap;
        }

        public void Dispose()
        {
            foreach (KeyValuePair<ulong, ReassemblyBuffer> kv in _reassembly)
            {
                kv.Value.Dispose();
            }

            _reassembly.Clear();
        }

        private static ulong Key(int chunkIndex, uint snapshotId)
        {
            return ((ulong)(uint)chunkIndex) | ((ulong)snapshotId << 32);
        }

        public bool OnChunkSnapshotFragBody(ref WorldChunkArray world, ReadOnlySpan<byte> body)
        {
            if (!ProtocolMessages.TryReadChunkSnapshotFrag(
                body,
                out short cx,
                out short cy,
                out uint snapshotId,
                out uint totalLen,
                out ushort fragIndex,
                out ushort fragCount,
                out ushort fragLen,
                out ushort codec,
                out ReadOnlySpan<byte> fragPayload))
            {
                return false;
            }

            if (codec != 1)
            {
                return false;
            }

            int chunkIndex = WorldConstants.ChunkIndex(cx, cy);
            ulong k = Key(chunkIndex, snapshotId);

            ReassemblyBuffer buf;
            if (!_reassembly.TryGetValue(k, out buf))
            {
                if (totalLen > 64 * 1024)
                {
                    return false;
                }

                buf = new ReassemblyBuffer((int)totalLen, fragCount);
                _reassembly.Add(k, buf);
            }

            int fragOffset = fragIndex * _fragPayloadCap;
            buf.TryAdd(fragIndex, fragPayload, fragOffset);

            if (!buf.IsComplete)
            {
                return false;
            }

            ReadOnlySpan<byte> compressed = buf.AsSpan();

            ChunkSnapshotCodec.FieldsMask mask =
                ChunkSnapshotCodec.FieldsMask.Height |
                ChunkSnapshotCodec.FieldsMask.RiverMask |
                ChunkSnapshotCodec.FieldsMask.Biome;

            int maxDecomp = ChunkSnapshotCodec.GetMaxDecompressedSize(mask);

            if (!ChunkSnapshotCodec.DecodeCompressed(
                compressed,
                _compressor,
                _decompressedScratch.AsSpan(0, maxDecomp),
                out ChunkSnapshotCodec.Header header,
                out ReadOnlySpan<byte> payload))
            {
                Cleanup(k);
                return false;
            }

            ChunkSoA c = world.GetChunk(cx, cy);
            if (!ChunkSnapshotCodec.ApplyDecodedPayloadToChunk(header, payload, ref c))
            {
                Cleanup(k);
                return false;
            }

            c.Dirty |= ChunkDirtyFlags.Derived | ChunkDirtyFlags.Render;
            world.SetChunk(cx, cy, c);

            Cleanup(k);
            return true;
        }

        private void Cleanup(ulong k)
        {
            ReassemblyBuffer b;
            if (_reassembly.TryGetValue(k, out b))
            {
                b.Dispose();
                _reassembly.Remove(k);
            }
        }
    }
}
