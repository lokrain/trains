#nullable enable
using System;
using Unity.Collections;
using OpenTTD.Core.Net.Protocol;
using OpenTTD.Core.World;

namespace OpenTTD.Core.Client.Net
{
    /// <summary>
    /// Minimal self-test for patch lineage validation and bounded resync request behavior.
    /// </summary>
    public static class WorldPatchReceiverSelfTest
    {
        public static bool Run()
        {
            var counters = new ReplicationCounters();
            var receiver = new WorldPatchReceiver(
                resyncCooldownTicks: 10,
                globalResyncWindowTicks: 100,
                globalResyncMaxRequests: 1,
                counters: counters);

            WorldChunkArray world = WorldChunkArray.Create(96, Allocator.Temp, Allocator.Temp);
            try
            {
                ChunkSoA chunk = world.GetChunk(0, 0);
                chunk.Versions.SnapshotVersion = 5;
                world.SetChunk(0, 0, chunk);

                Span<byte> writeBuf = stackalloc byte[64];
                Span<byte> payload = stackalloc byte[1];
                payload[0] = 123;

                int len = ProtocolMessages.WriteChunkPatchRect(
                    cx: 0,
                    cy: 0,
                    baseSnapshotId: 5,
                    newSnapshotId: 6,
                    rx: 0,
                    ry: 0,
                    rw: 1,
                    rh: 1,
                    fieldMask: 0x1,
                    patchCodec: (byte)HeightRectPatchCodec.Codec.AbsU8,
                    patchPayload: payload,
                    dst: writeBuf);

                if (!receiver.TryApplyPatchBody(
                    ref world,
                    writeBuf[..len],
                    nowTick: 1,
                    out bool shouldRequestResync,
                    out _,
                    out _,
                    out _,
                    out _))
                {
                    return false;
                }

                if (shouldRequestResync)
                {
                    return false;
                }

                chunk = world.GetChunk(0, 0);
                if (chunk.Versions.SnapshotVersion != 6 || chunk.Height[0] != 123)
                {
                    return false;
                }

                len = ProtocolMessages.WriteChunkPatchRect(
                    cx: 0,
                    cy: 0,
                    baseSnapshotId: 5,
                    newSnapshotId: 7,
                    rx: 0,
                    ry: 0,
                    rw: 1,
                    rh: 1,
                    fieldMask: 0x1,
                    patchCodec: (byte)HeightRectPatchCodec.Codec.AbsU8,
                    patchPayload: payload,
                    dst: writeBuf);

                if (receiver.TryApplyPatchBody(
                    ref world,
                    writeBuf[..len],
                    nowTick: 2,
                    out shouldRequestResync,
                    out short resyncCx,
                    out short resyncCy,
                    out uint expectedBase,
                    out uint localSnapshotId))
                {
                    return false;
                }

                if (!shouldRequestResync || receiver.LastErrorCode != ReplicationErrorCode.LineageMismatch)
                {
                    return false;
                }

                if (resyncCx != 0 || resyncCy != 0 || expectedBase != 5 || localSnapshotId != 6)
                {
                    return false;
                }

                if (receiver.TryApplyPatchBody(
                    ref world,
                    writeBuf[..len],
                    nowTick: 3,
                    out shouldRequestResync,
                    out _,
                    out _,
                    out _,
                    out _))
                {
                    return false;
                }

                if (shouldRequestResync)
                {
                    return false;
                }

                return counters.PatchApplies == 1
                    && counters.PatchMismatches == 2
                    && counters.ResyncRequestsIssued == 1
                    && counters.ResyncRequestsSuppressed == 1;
            }
            finally
            {
                world.Dispose();
            }
        }
    }
}
