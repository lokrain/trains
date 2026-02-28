#nullable enable
using System;

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Minimal protocol self-tests for replication control payloads.
    /// </summary>
    public static class ReplicationProtocolSelfTest
    {
        public static bool Run()
        {
            Span<byte> buf = stackalloc byte[32];

            int len = ProtocolMessages.WriteChunkResyncRequest(
                cx: 7,
                cy: 9,
                expectedBaseSnapshotId: 100u,
                clientSnapshotId: 98u,
                reason: ProtocolMessages.ResyncReasonCode.PatchBaseMismatch,
                dst: buf);

            if (!ProtocolMessages.TryReadChunkResyncRequest(
                buf.Slice(0, len),
                out short cx,
                out short cy,
                out uint expected,
                out uint client,
                out ProtocolMessages.ResyncReasonCode reason))
            {
                return false;
            }

            if (cx != 7 || cy != 9 || expected != 100u || client != 98u || reason != ProtocolMessages.ResyncReasonCode.PatchBaseMismatch)
            {
                return false;
            }

            if (!ProtocolMessages.TryReadResyncChunkRequest(buf.Slice(0, len), out cx, out cy, out client))
            {
                return false;
            }

            if (cx != 7 || cy != 9 || client != 98u)
            {
                return false;
            }

            buf[13] = 0xFF;
            if (ProtocolMessages.TryReadChunkResyncRequest(
                buf.Slice(0, len),
                out _,
                out _,
                out _,
                out _,
                out _))
            {
                return false;
            }

            return true;
        }
    }
}
