#nullable enable
using System;
using System.Runtime.InteropServices;

namespace OpenTTD.Core.WorldGen
{
    /// <summary>
    /// Authoritative world generation config.
    /// - Deterministic across platforms.
    /// - Serializable as a stable byte blob for multiplayer handshake and replay validation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldGenConfig : IEquatable<WorldGenConfig>
    {
        public uint WorldGenVersion;
        public ulong WorldSeed;

        public byte SeaLevel;
        public byte HeightCurve;
        public byte BaseAmplitude;
        public byte Reserved0;

        public ushort BaseGridTiles;
        public ushort Octave1GridTiles;
        public ushort Octave2GridTiles;
        public ushort Octave3GridTiles;

        public ushort W0_Q16;
        public ushort W1_Q16;
        public ushort W2_Q16;
        public ushort W3_Q16;

        public ushort WarpGridTiles;
        public ushort WarpStrengthQ8;

        public ushort RiverCount;
        public ushort RiverMaxSteps;
        public byte RiverMinSourceAboveSea;
        public byte RiverStampWidth;
        public ushort Reserved1;

        public byte EnableBiomes;
        public byte LatitudeBands;
        public byte AltitudeBands;
        public byte Reserved2;

        public byte SlopeClass1MaxDelta;
        public byte SlopeClass2MaxDelta;
        public byte SlopeClass3MaxDelta;
        public byte Reserved3;

        public byte MaxRailSlopeClassForStations;
        public byte MaxRailSlopeClassForTrack;
        public byte AllowTerraformOnRivers;
        public byte Reserved4;

        /// <summary>
        /// Creates deterministic default world generation settings.
        /// </summary>
        public static WorldGenConfig Default(ulong seed)
        {
            return new WorldGenConfig
            {
                WorldGenVersion = 1,
                WorldSeed = seed,

                SeaLevel = 96,
                HeightCurve = 1,
                BaseAmplitude = 255,

                BaseGridTiles = 256,
                Octave1GridTiles = 128,
                Octave2GridTiles = 64,
                Octave3GridTiles = 32,

                W0_Q16 = 29491,
                W1_Q16 = 16384,
                W2_Q16 = 11796,
                W3_Q16 = 7864,

                WarpGridTiles = 0,
                WarpStrengthQ8 = 0,

                RiverCount = 64,
                RiverMaxSteps = 4096,
                RiverMinSourceAboveSea = 48,
                RiverStampWidth = 1,

                EnableBiomes = 1,
                LatitudeBands = 8,
                AltitudeBands = 8,

                SlopeClass1MaxDelta = 1,
                SlopeClass2MaxDelta = 3,
                SlopeClass3MaxDelta = 6,

                MaxRailSlopeClassForStations = 1,
                MaxRailSlopeClassForTrack = 2,
                AllowTerraformOnRivers = 0
            };
        }

        public bool Equals(WorldGenConfig other)
        {
            return WorldGenVersion == other.WorldGenVersion
                && WorldSeed == other.WorldSeed
                && SeaLevel == other.SeaLevel
                && HeightCurve == other.HeightCurve
                && BaseAmplitude == other.BaseAmplitude
                && BaseGridTiles == other.BaseGridTiles
                && Octave1GridTiles == other.Octave1GridTiles
                && Octave2GridTiles == other.Octave2GridTiles
                && Octave3GridTiles == other.Octave3GridTiles
                && W0_Q16 == other.W0_Q16
                && W1_Q16 == other.W1_Q16
                && W2_Q16 == other.W2_Q16
                && W3_Q16 == other.W3_Q16
                && WarpGridTiles == other.WarpGridTiles
                && WarpStrengthQ8 == other.WarpStrengthQ8
                && RiverCount == other.RiverCount
                && RiverMaxSteps == other.RiverMaxSteps
                && RiverMinSourceAboveSea == other.RiverMinSourceAboveSea
                && RiverStampWidth == other.RiverStampWidth
                && EnableBiomes == other.EnableBiomes
                && LatitudeBands == other.LatitudeBands
                && AltitudeBands == other.AltitudeBands
                && SlopeClass1MaxDelta == other.SlopeClass1MaxDelta
                && SlopeClass2MaxDelta == other.SlopeClass2MaxDelta
                && SlopeClass3MaxDelta == other.SlopeClass3MaxDelta
                && MaxRailSlopeClassForStations == other.MaxRailSlopeClassForStations
                && MaxRailSlopeClassForTrack == other.MaxRailSlopeClassForTrack
                && AllowTerraformOnRivers == other.AllowTerraformOnRivers;
        }

        public override bool Equals(object obj)
        {
            return obj is WorldGenConfig other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(WorldGenVersion, WorldSeed, SeaLevel, BaseGridTiles, RiverCount);
        }

        public static bool operator ==(WorldGenConfig left, WorldGenConfig right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WorldGenConfig left, WorldGenConfig right)
        {
            return !left.Equals(right);
        }
    }
}
