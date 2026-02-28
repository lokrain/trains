#nullable enable
using System.Runtime.CompilerServices;

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Chunk snapshot fragmentation helpers.
    /// </summary>
    public static class ChunkFragmenter
    {
        /// <summary>
        /// Conservative datagram payload budget. Tune per transport pipeline.
        /// </summary>
        public const int MaxDatagramPayload = 1200;

        /// <summary>
        /// Snapshot fragment body header size:
        /// cx(i16)+cy(i16)+snapshot_id(u32)+frag_index(u16)+frag_count(u16)+frag_len(u16)+codec(u16) = 16.
        /// </summary>
        public const int FragBodyHeaderSize = 16;

        /// <summary>
        /// Computes max payload bytes available for one fragment body.
        /// </summary>
        /// <param name="envelopeOverheadBytes">Protocol envelope overhead in bytes.</param>
        /// <returns>Fragment payload capacity (minimum 64 bytes).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeFragPayloadCapacity(int envelopeOverheadBytes)
        {
            int cap = MaxDatagramPayload - envelopeOverheadBytes - FragBodyHeaderSize;
            return cap < 64 ? 64 : cap;
        }

        /// <summary>
        /// Computes required fragment count for a payload.
        /// </summary>
        /// <param name="totalLen">Total payload byte length.</param>
        /// <param name="fragPayloadCap">Per-fragment payload capacity.</param>
        /// <returns>Fragment count.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeFragCount(int totalLen, int fragPayloadCap)
        {
            return (totalLen + fragPayloadCap - 1) / fragPayloadCap;
        }
    }
}
