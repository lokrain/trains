#nullable enable
using System;
using System.Collections.Generic;
using OpenTTD.Core.Net.Protocol;
using OpenTTD.Core.World;

namespace OpenTTD.Core.Client.Net
{
    /// <summary>
    /// Applies chunk patch payloads with snapshot lineage checks.
    /// Requests resync when patch base does not match local chunk snapshot.
    /// </summary>
    public sealed class WorldPatchReceiver
    {
        private readonly Dictionary<int, ulong> _lastResyncRequestTickByChunk = new(128);
        private readonly ulong _resyncCooldownTicks;
        private readonly Queue<ulong> _recentResyncRequestTicks = new(32);
        private readonly ulong _globalResyncWindowTicks;
        private readonly int _globalResyncMaxRequests;
        private readonly ReplicationCounters? _counters;
        private ReplicationErrorCode _lastErrorCode;

        public WorldPatchReceiver(
            ulong resyncCooldownTicks = 30,
            ulong globalResyncWindowTicks = 30,
            int globalResyncMaxRequests = 8,
            ReplicationCounters? counters = null)
        {
            _resyncCooldownTicks = resyncCooldownTicks == 0 ? 30UL : resyncCooldownTicks;
            _globalResyncWindowTicks = globalResyncWindowTicks == 0 ? 30UL : globalResyncWindowTicks;
            _globalResyncMaxRequests = globalResyncMaxRequests <= 0 ? 8 : globalResyncMaxRequests;
            _counters = counters;
            _lastErrorCode = ReplicationErrorCode.None;
        }

        public ReplicationErrorCode LastErrorCode => _lastErrorCode;

        public bool TryApplyPatchBody(
            ref WorldChunkArray world,
            ReadOnlySpan<byte> body,
            ulong nowTick,
            out bool shouldRequestResync,
            out short resyncCx,
            out short resyncCy,
            out uint expectedBaseSnapshotId,
            out uint localSnapshotId)
        {
            shouldRequestResync = false;
            resyncCx = 0;
            resyncCy = 0;
            expectedBaseSnapshotId = 0;
            localSnapshotId = 0;
            _lastErrorCode = ReplicationErrorCode.None;

            if (!ProtocolMessages.TryReadChunkPatchRect(
                body,
                out short cx,
                out short cy,
                out uint baseSnapshotId,
                out uint newSnapshotId,
                out byte rx,
                out byte ry,
                out byte rw,
                out byte rh,
                out byte fieldMask,
                out byte patchCodec,
                out ReadOnlySpan<byte> patchPayload))
            {
                _lastErrorCode = ReplicationErrorCode.MalformedPayload;
                return false;
            }

            if ((uint)cx >= WorldConstants.ChunksW || (uint)cy >= WorldConstants.ChunksH)
            {
                _lastErrorCode = ReplicationErrorCode.MalformedPayload;
                return false;
            }

            int chunkIndex = WorldConstants.ChunkIndex(cx, cy);
            ChunkSoA chunk = world.GetChunk(cx, cy);
            localSnapshotId = chunk.Versions.SnapshotVersion;

            if (newSnapshotId <= baseSnapshotId)
            {
                _lastErrorCode = ReplicationErrorCode.InvalidTransferMetadata;
                return false;
            }

            if (localSnapshotId != baseSnapshotId)
            {
                shouldRequestResync = AllowResyncRequest(chunkIndex, nowTick);
                if (shouldRequestResync)
                {
                    _counters?.IncrementResyncRequestsIssued();
                }
                else
                {
                    _counters?.IncrementResyncRequestsSuppressed();
                }

                _counters?.IncrementPatchMismatches();
                _lastErrorCode = ReplicationErrorCode.LineageMismatch;
                resyncCx = cx;
                resyncCy = cy;
                expectedBaseSnapshotId = baseSnapshotId;
                return false;
            }

            int rectEndX = rx + rw;
            int rectEndY = ry + rh;
            if (rectEndX > WorldConstants.ChunkSize || rectEndY > WorldConstants.ChunkSize)
            {
                _lastErrorCode = ReplicationErrorCode.MalformedPayload;
                return false;
            }

            if ((fieldMask & 0x1) == 0)
            {
                _lastErrorCode = ReplicationErrorCode.InvalidTransferMetadata;
                return false;
            }

            int rectTiles = rw * rh;
            if (patchCodec == (byte)HeightRectPatchCodec.Codec.DeltaI8)
            {
                if (patchPayload.Length != rectTiles)
                {
                    _lastErrorCode = ReplicationErrorCode.MalformedPayload;
                    return false;
                }

                for (int y = 0; y < rh; y++)
                {
                    int baseIdx = (ry + y) * WorldConstants.ChunkSize + rx;
                    int srcBase = y * rw;
                    for (int x = 0; x < rw; x++)
                    {
                        int idx = baseIdx + x;
                        int v = chunk.Height[idx] + unchecked((sbyte)patchPayload[srcBase + x]);
                        if (v < 0)
                        {
                            v = 0;
                        }
                        else if (v > 255)
                        {
                            v = 255;
                        }

                        chunk.Height[idx] = (byte)v;
                    }
                }
            }
            else if (patchCodec == (byte)HeightRectPatchCodec.Codec.AbsU8)
            {
                if (patchPayload.Length != rectTiles)
                {
                    _lastErrorCode = ReplicationErrorCode.MalformedPayload;
                    return false;
                }

                for (int y = 0; y < rh; y++)
                {
                    int baseIdx = (ry + y) * WorldConstants.ChunkSize + rx;
                    int srcBase = y * rw;
                    for (int x = 0; x < rw; x++)
                    {
                        chunk.Height[baseIdx + x] = patchPayload[srcBase + x];
                    }
                }
            }
            else
            {
                _lastErrorCode = ReplicationErrorCode.UnsupportedCodec;
                return false;
            }

            chunk.Versions.SnapshotVersion = newSnapshotId;
            chunk.Versions.HeightVersion = newSnapshotId;
            chunk.Dirty |= ChunkDirtyFlags.Derived | ChunkDirtyFlags.Snapshot | ChunkDirtyFlags.Render;
            chunk.MarkDirtyTile(rx, ry);
            chunk.MarkDirtyTile((byte)(rx + rw - 1), (byte)(ry + rh - 1));
            world.SetChunk(cx, cy, chunk);
            _counters?.IncrementPatchApplies();
            return true;
        }

        private bool AllowResyncRequest(int chunkIndex, ulong nowTick)
        {
            while (_recentResyncRequestTicks.Count > 0)
            {
                ulong first = _recentResyncRequestTicks.Peek();
                if (nowTick - first < _globalResyncWindowTicks)
                {
                    break;
                }

                _recentResyncRequestTicks.Dequeue();
            }

            if (_recentResyncRequestTicks.Count >= _globalResyncMaxRequests)
            {
                return false;
            }

            if (_lastResyncRequestTickByChunk.TryGetValue(chunkIndex, out ulong last))
            {
                if (nowTick - last < _resyncCooldownTicks)
                {
                    return false;
                }
            }

            _lastResyncRequestTickByChunk[chunkIndex] = nowTick;
            _recentResyncRequestTicks.Enqueue(nowTick);
            return true;
        }
    }
}
