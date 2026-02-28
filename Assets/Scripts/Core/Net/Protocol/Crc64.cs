#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Deterministic CRC64-ECMA (poly 0x42F0E1EBA9EA3693).
    /// Table-less implementation with zero static table init.
    /// </summary>
    public static class Crc64
    {
        private const ulong Poly = 0x42F0E1EBA9EA3693ul;

        /// <summary>
        /// Computes CRC64-ECMA over source bytes.
        /// </summary>
        /// <param name="data">Input bytes.</param>
        /// <returns>CRC64 checksum.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Compute(ReadOnlySpan<byte> data)
        {
            ulong crc = 0;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= (ulong)data[i] << 56;
                for (int b = 0; b < 8; b++)
                {
                    bool msb = (crc & 0x8000000000000000ul) != 0;
                    crc <<= 1;
                    if (msb)
                    {
                        crc ^= Poly;
                    }
                }
            }

            return crc;
        }
    }
}
