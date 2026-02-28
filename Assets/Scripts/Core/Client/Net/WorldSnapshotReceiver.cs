#nullable enable
using System;
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
        private readonly ReplicationCounters? _counters;
        private ReplicationErrorCode _lastErrorCode;

        private readonly SnapshotReassemblyManager _reassembly;
        private readonly byte[] _decompressedScratch = new byte[8 + 4096 * 3];

        public WorldSnapshotReceiver(
            ICompressor compressor,
            int fragPayloadCap,
            ulong transferTimeoutTicks = 300,
            ReplicationCounters? counters = null)
        {
            _compressor = compressor;
            _fragPayloadCap = fragPayloadCap;
            _reassembly = new SnapshotReassemblyManager(transferTimeoutTicks);
            _counters = counters;
            _lastErrorCode = ReplicationErrorCode.None;
        }

        public ReplicationErrorCode LastErrorCode => _lastErrorCode;

        public void Dispose()
        {
            _reassembly.Dispose();
        }

        private static ulong Key(int chunkIndex, uint snapshotId)
        {
            return ((ulong)(uint)chunkIndex) | ((ulong)snapshotId << 32);
        }

        public int EvictExpiredTransfers(ulong nowTick)
        {
            int evicted = _reassembly.EvictExpired(nowTick);
            _counters?.AddSnapshotTransferTimeoutEvictions(evicted);
            return evicted;
        }

        public bool OnChunkSnapshotFragBody(ref WorldChunkArray world, ReadOnlySpan<byte> body, ulong nowTick)
        {
            _lastErrorCode = ReplicationErrorCode.None;

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
                _lastErrorCode = ReplicationErrorCode.MalformedPayload;
                return false;
            }

            _counters?.IncrementSnapshotFragmentsReceived();

            if (codec != 1)
            {
                _lastErrorCode = ReplicationErrorCode.UnsupportedCodec;
                return false;
            }

            if (_fragPayloadCap <= 0)
            {
                _lastErrorCode = ReplicationErrorCode.InvalidTransferMetadata;
                return false;
            }

            if (totalLen == 0 || totalLen > int.MaxValue)
            {
                _lastErrorCode = ReplicationErrorCode.InvalidTransferMetadata;
                return false;
            }

            if (fragCount == 0)
            {
                _lastErrorCode = ReplicationErrorCode.InvalidTransferMetadata;
                return false;
            }

            if (fragLen == 0 || fragLen > _fragPayloadCap)
            {
                _lastErrorCode = ReplicationErrorCode.InvalidTransferMetadata;
                return false;
            }

            if (fragPayload.Length != fragLen)
            {
                _lastErrorCode = ReplicationErrorCode.MalformedPayload;
                return false;
            }

            long maxCoveredLen = (long)fragCount * _fragPayloadCap;
            if (totalLen > maxCoveredLen)
            {
                _lastErrorCode = ReplicationErrorCode.InvalidTransferMetadata;
                return false;
            }

            int chunkIndex = WorldConstants.ChunkIndex(cx, cy);
            ulong k = Key(chunkIndex, snapshotId);

            if (!_reassembly.TryGetOrCreate(k, (int)totalLen, fragCount, nowTick, out ReassemblyBuffer? buf) || buf == null)
            {
                _lastErrorCode = ReplicationErrorCode.ReassemblyCreateFailed;
                return false;
            }

            long fragOffsetLong = (long)fragIndex * _fragPayloadCap;
            if (fragOffsetLong < 0 || fragOffsetLong > int.MaxValue)
            {
                _lastErrorCode = ReplicationErrorCode.InvalidTransferMetadata;
                Cleanup(k);
                return false;
            }

            long fragEnd = fragOffsetLong + fragLen;
            if (fragEnd > totalLen)
            {
                _lastErrorCode = ReplicationErrorCode.InvalidTransferMetadata;
                Cleanup(k);
                return false;
            }

            int fragOffset = (int)fragOffsetLong;
            if (!buf.TryAdd(fragIndex, fragPayload, fragOffset))
            {
                _counters?.IncrementSnapshotReassemblyFailures();
                _lastErrorCode = ReplicationErrorCode.ReassemblyAddFailed;
                Cleanup(k);
                return false;
            }

            _reassembly.Touch(k, nowTick);

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
                _counters?.IncrementSnapshotReassemblyFailures();
                _lastErrorCode = ReplicationErrorCode.DecodeFailed;
                Cleanup(k);
                return false;
            }

            ChunkSoA c = world.GetChunk(cx, cy);
            if (!ChunkSnapshotCodec.ApplyDecodedPayloadToChunk(header, payload, ref c))
            {
                _counters?.IncrementSnapshotReassemblyFailures();
                _lastErrorCode = ReplicationErrorCode.ApplyFailed;
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
            _reassembly.Remove(k);
        }
    }
}
