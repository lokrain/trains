#nullable enable
using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Height-rect patch encoding helpers for protocol payloads.
    /// </summary>
    public static class HeightRectPatchCodec
    {
        /// <summary>
        /// Patch payload codec identifiers.
        /// </summary>
        public enum Codec : byte
        {
            AbsU8 = 0,
            DeltaI8 = 1,
            RleDelta = 2
        }

        /// <summary>
        /// Computes tile count inside a rect.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RectTileCount(byte rw, byte rh)
        {
            return rw * rh;
        }

        /// <summary>
        /// Encodes signed deltas as raw i8 bytes.
        /// </summary>
        public static int EncodeDeltaI8(ReadOnlySpan<sbyte> deltas, Span<byte> dst)
        {
            for (int i = 0; i < deltas.Length; i++)
            {
                dst[i] = unchecked((byte)deltas[i]);
            }

            return deltas.Length;
        }

        /// <summary>
        /// Decodes raw i8 bytes to signed deltas.
        /// </summary>
        public static int DecodeDeltaI8(ReadOnlySpan<byte> src, Span<sbyte> deltasOut)
        {
            for (int i = 0; i < deltasOut.Length; i++)
            {
                deltasOut[i] = unchecked((sbyte)src[i]);
            }

            return deltasOut.Length;
        }

        /// <summary>
        /// Applies signed deltas to a 64x64 height tile block with clamping.
        /// </summary>
        public static void ApplyDeltaToHeights(
            Span<byte> heights,
            byte rx,
            byte ry,
            byte rw,
            byte rh,
            ReadOnlySpan<sbyte> deltas)
        {
            int k = 0;
            for (int y = 0; y < rh; y++)
            {
                int baseIdx = (ry + y) * 64 + rx;
                for (int x = 0; x < rw; x++)
                {
                    int idx = baseIdx + x;
                    int v = heights[idx] + deltas[k++];
                    heights[idx] = (byte)math.clamp(v, 0, 255);
                }
            }
        }
    }
}
