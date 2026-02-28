#nullable enable
using System;

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Compression adapter contract for protocol payload encoding/decoding.
    /// </summary>
    public interface ICompressor
    {
        /// <summary>
        /// Compresses source bytes into destination span.
        /// </summary>
        /// <param name="src">Uncompressed source bytes.</param>
        /// <param name="dst">Destination buffer for compressed bytes.</param>
        /// <returns>Bytes written to <paramref name="dst" />.</returns>
        int Compress(ReadOnlySpan<byte> src, Span<byte> dst);

        /// <summary>
        /// Decompresses source bytes into destination span.
        /// </summary>
        /// <param name="src">Compressed source bytes.</param>
        /// <param name="dst">Destination buffer for decompressed bytes.</param>
        /// <returns>Bytes written to <paramref name="dst" />.</returns>
        int Decompress(ReadOnlySpan<byte> src, Span<byte> dst);
    }
}
