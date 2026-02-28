#nullable enable
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using OpenTTD.Core.World;
using OpenTTD.Core.WorldGen;

namespace OpenTTD.Core.Terraform
{
    /// <summary>
    /// Recomputes derived slope/build-mask after height changes. Sea is derived from height.
    /// </summary>
    [BurstCompile]
    public struct RecomputeDerivedForChunkJob : IJobParallelFor
    {
        public int ChunkX;
        public int ChunkY;
        public WorldChunkArray World;
        public WorldGenConfig Config;

        [ReadOnly] public NativeArray<byte> RiverMask;
        public NativeArray<byte> OutSlope;
        public NativeArray<ushort> OutBuildMask;

        public void Execute(int index)
        {
            int lx = index & 63;
            int ly = index >> 6;

            int wx = (ChunkX << 6) + lx;
            int wy = (ChunkY << 6) + ly;

            ChunkSoA c = World.GetChunk(ChunkX, ChunkY);
            byte hC = c.Height[index];

            byte hN = TileAccessor.GetHeightClamped(ref World, wx, wy - 1);
            byte hS = TileAccessor.GetHeightClamped(ref World, wx, wy + 1);
            byte hE = TileAccessor.GetHeightClamped(ref World, wx + 1, wy);
            byte hW = TileAccessor.GetHeightClamped(ref World, wx - 1, wy);

            byte slopeClass = TileAccessor.ComputeSlopeClass(
                hC,
                hN,
                hS,
                hE,
                hW,
                Config.SlopeClass1MaxDelta,
                Config.SlopeClass2MaxDelta,
                Config.SlopeClass3MaxDelta);
            OutSlope[index] = slopeClass;

            bool isRiver = RiverMask[index] != 0;
            bool isSea = !isRiver && hC <= World.SeaLevel;

            ushort mask = 0;

            if (isSea)
            {
                mask |= BuildMaskBits.IsSea;
            }

            if (isRiver)
            {
                mask |= BuildMaskBits.IsRiver;
            }

            if (!isRiver || Config.AllowTerraformOnRivers != 0)
            {
                mask |= BuildMaskBits.CanTerraform;
            }

            if (!isSea && !isRiver)
            {
                if (slopeClass <= Config.MaxRailSlopeClassForStations)
                {
                    mask |= BuildMaskBits.CanPlaceRail | BuildMaskBits.CanPlaceStation;
                }

                if (slopeClass > Config.MaxRailSlopeClassForStations && slopeClass <= Config.MaxRailSlopeClassForTrack)
                {
                    mask |= BuildMaskBits.CanPlaceRail;
                }

                if (slopeClass > Config.MaxRailSlopeClassForTrack)
                {
                    mask |= BuildMaskBits.CanTunnelCandidate;
                }
            }
            else if (isSea)
            {
                mask |= BuildMaskBits.CanBridgeCandidate;
            }

            mask |= (ushort)(slopeClass << BuildMaskBits.SlopeClassShift);
            OutBuildMask[index] = mask;
        }
    }
}
