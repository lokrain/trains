#nullable enable

namespace OpenTTD.Core.WorldGen
{
    /// <summary>
    /// Deterministic worldgen config validation error codes.
    /// </summary>
    public enum WorldGenConfigError : ushort
    {
        None = 0,
        UnsupportedVersion = 1,
        SeaLevelOutOfRange = 2,
        InvalidGridSizes = 3,
        InvalidWeights = 4,
        InvalidRiverSettings = 5,
        InvalidSlopeThresholds = 6,
        InvalidRailSlopeRules = 7,
        InvalidBiomeBands = 8
    }

    /// <summary>
    /// Validation helpers for authoritative worldgen config schema.
    /// </summary>
    public static class WorldGenConfigValidation
    {
        public static bool TryValidate(in WorldGenConfig cfg, out WorldGenConfigError error)
        {
            if (cfg.WorldGenVersion != 1)
            {
                error = WorldGenConfigError.UnsupportedVersion;
                return false;
            }

            if (cfg.BaseGridTiles == 0 || cfg.Octave1GridTiles == 0 || cfg.Octave2GridTiles == 0 || cfg.Octave3GridTiles == 0)
            {
                error = WorldGenConfigError.InvalidGridSizes;
                return false;
            }

            if (cfg.W0_Q16 == 0 && cfg.W1_Q16 == 0 && cfg.W2_Q16 == 0 && cfg.W3_Q16 == 0)
            {
                error = WorldGenConfigError.InvalidWeights;
                return false;
            }

            if (cfg.RiverMaxSteps == 0 || cfg.RiverCount == 0)
            {
                error = WorldGenConfigError.InvalidRiverSettings;
                return false;
            }

            if (!(cfg.SlopeClass1MaxDelta <= cfg.SlopeClass2MaxDelta && cfg.SlopeClass2MaxDelta <= cfg.SlopeClass3MaxDelta))
            {
                error = WorldGenConfigError.InvalidSlopeThresholds;
                return false;
            }

            if (cfg.MaxRailSlopeClassForStations > cfg.MaxRailSlopeClassForTrack)
            {
                error = WorldGenConfigError.InvalidRailSlopeRules;
                return false;
            }

            if (cfg.EnableBiomes != 0 && (cfg.LatitudeBands == 0 || cfg.AltitudeBands == 0))
            {
                error = WorldGenConfigError.InvalidBiomeBands;
                return false;
            }

            error = WorldGenConfigError.None;
            return true;
        }
    }
}
