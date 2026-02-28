#nullable enable
using System;
using System.Runtime.CompilerServices;

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
        public const int SizeBytes =
            4 + // WorldGenVersion
            8 + // WorldSeed
            4 + // SeaLevel..Reserved0
            8 + // grid sizes
            8 + // weights
            4 + // warp
            8 + // river block
            4 + // biome block
            4 + // slope thresholds block
            4;  // build rules block

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

            int o = 0;
            WriteU32(dst, ref o, cfg.WorldGenVersion);
            WriteU64(dst, ref o, cfg.WorldSeed);

            dst[o++] = cfg.SeaLevel;
            dst[o++] = cfg.HeightCurve;
            dst[o++] = cfg.BaseAmplitude;
            dst[o++] = cfg.Reserved0;

            WriteU16(dst, ref o, cfg.BaseGridTiles);
            WriteU16(dst, ref o, cfg.Octave1GridTiles);
            WriteU16(dst, ref o, cfg.Octave2GridTiles);
            WriteU16(dst, ref o, cfg.Octave3GridTiles);

            WriteU16(dst, ref o, cfg.W0_Q16);
            WriteU16(dst, ref o, cfg.W1_Q16);
            WriteU16(dst, ref o, cfg.W2_Q16);
            WriteU16(dst, ref o, cfg.W3_Q16);

            WriteU16(dst, ref o, cfg.WarpGridTiles);
            WriteU16(dst, ref o, cfg.WarpStrengthQ8);

            WriteU16(dst, ref o, cfg.RiverCount);
            WriteU16(dst, ref o, cfg.RiverMaxSteps);
            dst[o++] = cfg.RiverMinSourceAboveSea;
            dst[o++] = cfg.RiverStampWidth;
            WriteU16(dst, ref o, cfg.Reserved1);

            dst[o++] = cfg.EnableBiomes;
            dst[o++] = cfg.LatitudeBands;
            dst[o++] = cfg.AltitudeBands;
            dst[o++] = cfg.Reserved2;

            dst[o++] = cfg.SlopeClass1MaxDelta;
            dst[o++] = cfg.SlopeClass2MaxDelta;
            dst[o++] = cfg.SlopeClass3MaxDelta;
            dst[o++] = cfg.Reserved3;

            dst[o++] = cfg.MaxRailSlopeClassForStations;
            dst[o++] = cfg.MaxRailSlopeClassForTrack;
            dst[o++] = cfg.AllowTerraformOnRivers;
            dst[o++] = cfg.Reserved4;
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

            int o = 0;
            WorldGenConfig cfg = default;
            cfg.WorldGenVersion = ReadU32(src, ref o);
            cfg.WorldSeed = ReadU64(src, ref o);

            cfg.SeaLevel = src[o++];
            cfg.HeightCurve = src[o++];
            cfg.BaseAmplitude = src[o++];
            cfg.Reserved0 = src[o++];

            cfg.BaseGridTiles = ReadU16(src, ref o);
            cfg.Octave1GridTiles = ReadU16(src, ref o);
            cfg.Octave2GridTiles = ReadU16(src, ref o);
            cfg.Octave3GridTiles = ReadU16(src, ref o);

            cfg.W0_Q16 = ReadU16(src, ref o);
            cfg.W1_Q16 = ReadU16(src, ref o);
            cfg.W2_Q16 = ReadU16(src, ref o);
            cfg.W3_Q16 = ReadU16(src, ref o);

            cfg.WarpGridTiles = ReadU16(src, ref o);
            cfg.WarpStrengthQ8 = ReadU16(src, ref o);

            cfg.RiverCount = ReadU16(src, ref o);
            cfg.RiverMaxSteps = ReadU16(src, ref o);
            cfg.RiverMinSourceAboveSea = src[o++];
            cfg.RiverStampWidth = src[o++];
            cfg.Reserved1 = ReadU16(src, ref o);

            cfg.EnableBiomes = src[o++];
            cfg.LatitudeBands = src[o++];
            cfg.AltitudeBands = src[o++];
            cfg.Reserved2 = src[o++];

            cfg.SlopeClass1MaxDelta = src[o++];
            cfg.SlopeClass2MaxDelta = src[o++];
            cfg.SlopeClass3MaxDelta = src[o++];
            cfg.Reserved3 = src[o++];

            cfg.MaxRailSlopeClassForStations = src[o++];
            cfg.MaxRailSlopeClassForTrack = src[o++];
            cfg.AllowTerraformOnRivers = src[o++];
            cfg.Reserved4 = src[o++];

            return cfg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteU16(Span<byte> dst, ref int o, ushort v)
        {
            dst[o + 0] = (byte)v;
            dst[o + 1] = (byte)(v >> 8);
            o += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort ReadU16(ReadOnlySpan<byte> src, ref int o)
        {
            ushort v = (ushort)(src[o + 0] | (src[o + 1] << 8));
            o += 2;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteU32(Span<byte> dst, ref int o, uint v)
        {
            dst[o + 0] = (byte)v;
            dst[o + 1] = (byte)(v >> 8);
            dst[o + 2] = (byte)(v >> 16);
            dst[o + 3] = (byte)(v >> 24);
            o += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ReadU32(ReadOnlySpan<byte> src, ref int o)
        {
            uint v = (uint)(src[o + 0] | (src[o + 1] << 8) | (src[o + 2] << 16) | (src[o + 3] << 24));
            o += 4;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteU64(Span<byte> dst, ref int o, ulong v)
        {
            dst[o + 0] = (byte)v;
            dst[o + 1] = (byte)(v >> 8);
            dst[o + 2] = (byte)(v >> 16);
            dst[o + 3] = (byte)(v >> 24);
            dst[o + 4] = (byte)(v >> 32);
            dst[o + 5] = (byte)(v >> 40);
            dst[o + 6] = (byte)(v >> 48);
            dst[o + 7] = (byte)(v >> 56);
            o += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ReadU64(ReadOnlySpan<byte> src, ref int o)
        {
            ulong v =
                ((ulong)src[o + 0])
                | ((ulong)src[o + 1] << 8)
                | ((ulong)src[o + 2] << 16)
                | ((ulong)src[o + 3] << 24)
                | ((ulong)src[o + 4] << 32)
                | ((ulong)src[o + 5] << 40)
                | ((ulong)src[o + 6] << 48)
                | ((ulong)src[o + 7] << 56);
            o += 8;
            return v;
        }
    }
}
