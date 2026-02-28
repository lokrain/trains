#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenTTD.Core.WorldGen
{
    /// <summary>
    /// Deterministic serialization helper for <see cref="WorldGenConfig" />.
    /// Blob bytes are used for handshake payload and config checksum inputs.
    /// </summary>
    public static class WorldGenConfigBlob
    {
        /// <summary>
        /// Size of serialized config blob in bytes.
        /// </summary>
        public static int SizeBytes => Marshal.SizeOf<WorldGenConfig>();

        /// <summary>
        /// Writes config bytes into destination span.
        /// </summary>
        /// <param name="cfg">Source worldgen config.</param>
        /// <param name="dst">Destination byte span.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(in WorldGenConfig cfg, Span<byte> dst)
        {
            if (dst.Length < SizeBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(dst));
            }

            WorldGenConfig value = cfg;
            MemoryMarshal.Write(dst, ref value);
        }

        /// <summary>
        /// Reads config bytes from source span.
        /// </summary>
        /// <param name="src">Source byte span.</param>
        /// <returns>Deserialized config value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WorldGenConfig Read(ReadOnlySpan<byte> src)
        {
            if (src.Length < SizeBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(src));
            }

            return MemoryMarshal.Read<WorldGenConfig>(src);
        }
    }
}
