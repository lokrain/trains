#nullable enable
using OpenTTD.Core.World;
using OpenTTD.Core.WorldGen;

namespace OpenTTD.Infra.Determinism
{
    /// <summary>
    /// Determinism harness for worldgen hash reproducibility across different batch settings.
    /// </summary>
    public static class WorldGenDeterminismRunner
    {
        public static ulong ComputeWorldHash(in WorldChunkArray world)
        {
            ulong hash = DeterministicHashing.Seed(world.SeaLevel);

            for (int chunkIndex = 0; chunkIndex < WorldConstants.ChunkCount; chunkIndex++)
            {
                ChunkSoA chunk = world.GetChunk(chunkIndex);
                hash = DeterministicHashing.Combine(hash, (uint)chunkIndex);

                for (int i = 0; i < chunk.Height.Length; i++)
                {
                    hash = DeterministicHashing.Combine(hash, chunk.Height[i]);
                    hash = DeterministicHashing.Combine(hash, chunk.RiverMask[i]);
                    hash = DeterministicHashing.Combine(hash, chunk.Biome[i]);
                    hash = DeterministicHashing.Combine(hash, chunk.Slope[i]);
                    hash = DeterministicHashing.Combine(hash, chunk.BuildMask[i]);
                }
            }

            return hash;
        }

        public static bool CompareBatchSettings(in WorldGenConfig config, int aBatchSize, int bBatchSize, out ulong hashA, out ulong hashB)
        {
            WorldChunkArray worldA = WorldGenOrchestrator.GenerateWorld(config, aBatchSize);
            WorldChunkArray worldB = WorldGenOrchestrator.GenerateWorld(config, bBatchSize);

            try
            {
                hashA = ComputeWorldHash(worldA);
                hashB = ComputeWorldHash(worldB);
                return hashA == hashB;
            }
            finally
            {
                worldA.Dispose();
                worldB.Dispose();
            }
        }
    }
}
