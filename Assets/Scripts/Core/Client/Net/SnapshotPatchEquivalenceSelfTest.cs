#nullable enable
using System;
using Unity.Collections;
using OpenTTD.Core.Net.Protocol;
using OpenTTD.Core.World;

namespace OpenTTD.Core.Client.Net
{
    /// <summary>
    /// Minimal self-test ensuring patch application converges to expected authoritative chunk bytes.
    /// </summary>
    public static class SnapshotPatchEquivalenceSelfTest
    {
        public static bool Run()
        {
            var receiver = new WorldPatchReceiver();

            WorldChunkArray patched = WorldChunkArray.Create(96, Allocator.Temp, Allocator.Temp);
            WorldChunkArray expected = WorldChunkArray.Create(96, Allocator.Temp, Allocator.Temp);

            try
            {
                ChunkSoA chunkA = patched.GetChunk(0, 0);
                ChunkSoA chunkB = expected.GetChunk(0, 0);

                for (int i = 0; i < chunkA.Height.Length; i++)
                {
                    byte value = (byte)(i & 0xFF);
                    chunkA.Height[i] = value;
                    chunkB.Height[i] = value;
                }

                chunkA.Versions.SnapshotVersion = 10;
                chunkB.Versions.SnapshotVersion = 10;
                patched.SetChunk(0, 0, chunkA);
                expected.SetChunk(0, 0, chunkB);

                Span<byte> payload = stackalloc byte[4];
                payload[0] = 7;
                payload[1] = 8;
                payload[2] = 9;
                payload[3] = 10;

                Span<byte> body = stackalloc byte[64];
                int bodyLen = ProtocolMessages.WriteChunkPatchRect(
                    cx: 0,
                    cy: 0,
                    baseSnapshotId: 10,
                    newSnapshotId: 11,
                    rx: 1,
                    ry: 1,
                    rw: 2,
                    rh: 2,
                    fieldMask: 0x1,
                    patchCodec: (byte)HeightRectPatchCodec.Codec.AbsU8,
                    patchPayload: payload,
                    dst: body);

                if (!receiver.TryApplyPatchBody(
                    ref patched,
                    body[..bodyLen],
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

                chunkB = expected.GetChunk(0, 0);
                int k = 0;
                for (int y = 1; y < 3; y++)
                {
                    int baseIdx = y * WorldConstants.ChunkSize + 1;
                    for (int x = 0; x < 2; x++)
                    {
                        chunkB.Height[baseIdx + x] = payload[k++];
                    }
                }

                chunkB.Versions.SnapshotVersion = 11;
                chunkB.Versions.HeightVersion = 11;
                expected.SetChunk(0, 0, chunkB);

                chunkA = patched.GetChunk(0, 0);
                chunkB = expected.GetChunk(0, 0);

                if (chunkA.Versions.SnapshotVersion != chunkB.Versions.SnapshotVersion)
                {
                    return false;
                }

                for (int i = 0; i < chunkA.Height.Length; i++)
                {
                    if (chunkA.Height[i] != chunkB.Height[i])
                    {
                        return false;
                    }
                }

                return true;
            }
            finally
            {
                patched.Dispose();
                expected.Dispose();
            }
        }
    }
}
