#nullable enable
using System;
using Unity.Collections;
using OpenTTD.Core.Net.Protocol;
using OpenTTD.Core.World;

namespace OpenTTD.Core.Client.Net
{
    /// <summary>
    /// Minimal property-style self-test for patch lineage chain acceptance/rejection behavior.
    /// </summary>
    public static class PatchChainPropertySelfTest
    {
        public static bool Run()
        {
            var receiver = new WorldPatchReceiver();
            WorldChunkArray world = WorldChunkArray.Create(96, Allocator.Temp, Allocator.Temp);

            try
            {
                ChunkSoA chunk = world.GetChunk(0, 0);
                chunk.Versions.SnapshotVersion = 1;
                world.SetChunk(0, 0, chunk);

                Span<byte> payload = stackalloc byte[1];
                Span<byte> body = stackalloc byte[64];

                for (uint step = 1; step <= 8; step++)
                {
                    payload[0] = (byte)(20 + step);
                    int len = ProtocolMessages.WriteChunkPatchRect(
                        cx: 0,
                        cy: 0,
                        baseSnapshotId: step,
                        newSnapshotId: step + 1,
                        rx: 0,
                        ry: 0,
                        rw: 1,
                        rh: 1,
                        fieldMask: 0x1,
                        patchCodec: (byte)HeightRectPatchCodec.Codec.AbsU8,
                        patchPayload: payload,
                        dst: body);

                    if (!receiver.TryApplyPatchBody(
                        ref world,
                        body.Slice(0, len),
                        nowTick: step,
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
                }

                chunk = world.GetChunk(0, 0);
                if (chunk.Versions.SnapshotVersion != 9)
                {
                    return false;
                }

                payload[0] = 200;
                int badLen = ProtocolMessages.WriteChunkPatchRect(
                    cx: 0,
                    cy: 0,
                    baseSnapshotId: 7,
                    newSnapshotId: 10,
                    rx: 0,
                    ry: 0,
                    rw: 1,
                    rh: 1,
                    fieldMask: 0x1,
                    patchCodec: (byte)HeightRectPatchCodec.Codec.AbsU8,
                    patchPayload: payload,
                    dst: body);

                if (receiver.TryApplyPatchBody(
                    ref world,
                    body.Slice(0, badLen),
                    nowTick: 20,
                    out bool shouldRequestResyncOnSkip,
                    out _,
                    out _,
                    out _,
                    out _))
                {
                    return false;
                }

                return shouldRequestResyncOnSkip
                    && receiver.LastErrorCode == ReplicationErrorCode.LineageMismatch;
            }
            finally
            {
                world.Dispose();
            }
        }
    }
}
