#nullable enable
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace OpenTTD.Core.WorldGen
{
    /// <summary>
    /// Biome identifiers for world generation output.
    /// </summary>
    public static class Biomes
    {
        public const byte Plains = 0;
        public const byte Hills = 1;
        public const byte Mountains = 2;
        public const byte Coast = 3;
        public const byte Riverland = 4;
    }

    /// <summary>
    /// Generates static biome ids from height, river mask, and sea-level thresholds.
    /// </summary>
    [BurstCompile]
    public struct BiomeGenJob : IJobParallelFor
    {
        public int ChunkX;
        public int ChunkY;
        public byte SeaLevel;

        [ReadOnly] public NativeArray<byte> Height;
        [ReadOnly] public NativeArray<byte> RiverMask;
        public NativeArray<byte> OutBiome;

        public void Execute(int index)
        {
            byte h = Height[index];
            bool isRiver = RiverMask[index] != 0;
            bool isSea = !isRiver && h <= SeaLevel;

            if (isSea)
            {
                OutBiome[index] = Biomes.Coast;
                return;
            }

            if (isRiver)
            {
                OutBiome[index] = Biomes.Riverland;
                return;
            }

            if (h >= SeaLevel + 96)
            {
                OutBiome[index] = Biomes.Mountains;
                return;
            }

            if (h >= SeaLevel + 48)
            {
                OutBiome[index] = Biomes.Hills;
                return;
            }

            OutBiome[index] = Biomes.Plains;
        }
    }
}
