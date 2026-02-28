#nullable enable
using System;
using OpenTTD.Core.Net.Protocol;
using OpenTTD.Core.WorldGen;

namespace OpenTTD.Core.Client.Net
{
    /// <summary>
    /// Validates server hello payload and world-generation config checksum.
    /// </summary>
    public static class HandshakeValidator
    {
        /// <summary>
        /// Validates server hello body and CRC64 of embedded worldgen config blob.
        /// </summary>
        /// <param name="serverHelloBody">Decoded ServerHello payload body bytes.</param>
        /// <param name="cfg">Parsed worldgen config when valid.</param>
        /// <returns>True when payload parses and config checksum matches; otherwise false.</returns>
        public static bool ValidateServerHello(ReadOnlySpan<byte> serverHelloBody, out WorldGenConfig cfg)
        {
            cfg = default;

            if (!ProtocolMessages.TryReadServerHello(serverHelloBody, out cfg, out _, out ulong cfgCrc))
            {
                return false;
            }

            Span<byte> blob = stackalloc byte[WorldGenConfigBlob.SizeBytes];
            WorldGenConfigBlob.Write(cfg, blob);

            ulong localCrc = Crc64.Compute(blob);
            return localCrc == cfgCrc;
        }
    }
}
