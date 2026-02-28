#nullable enable

namespace OpenTTD.Core.World
{
    /// <summary>
    /// Build-mask bit definitions used for terrain/build validation rules.
    /// </summary>
    public static class BuildMaskBits
    {
        public const ushort IsSea = 1 << 0;
        public const ushort IsRiver = 1 << 1;

        public const ushort CanTerraform = 1 << 2;

        public const ushort CanPlaceRail = 1 << 3;
        public const ushort CanPlaceStation = 1 << 4;

        public const ushort CanBridgeCandidate = 1 << 5;
        public const ushort CanTunnelCandidate = 1 << 6;

        public const ushort SlopeClassShift = 8;
    }
}
