#nullable enable
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace OpenTTD.Core.WorldGen
{
    /// <summary>
    /// Static river generation for a single chunk.
    /// This job is intentionally single-thread per chunk for determinism and simplicity.
    /// Parallelization should happen across chunks.
    /// </summary>
    [BurstCompile]
    public struct RiverGenJob : IJob
    {
        public ulong SeedRivers;
        public int ChunkX;
        public int ChunkY;
        public byte SeaLevel;

        [ReadOnly] public NativeArray<byte> Height;
        public NativeArray<byte> RiverMask;
        public int RiversPerChunk;

        public void Execute()
        {
            if (RiversPerChunk <= 0)
            {
                return;
            }

            var rng = new SplitMix64(Hash64.Hash(SeedRivers, (ulong)(uint)ChunkX, (ulong)(uint)ChunkY));

            for (int r = 0; r < RiversPerChunk; r++)
            {
                int bestIdx = -1;
                byte bestH = 0;

                const int K = 64;
                for (int i = 0; i < K; i++)
                {
                    int idx = (int)(rng.NextU32() & 4095u);
                    byte h = Height[idx];
                    if (h > bestH && h >= (byte)(SeaLevel + 32))
                    {
                        bestH = h;
                        bestIdx = idx;
                    }
                }

                if (bestIdx < 0)
                {
                    continue;
                }

                CarveRiverFrom(bestIdx, ref rng);
            }
        }

        private void CarveRiverFrom(int startIdx, ref SplitMix64 rng)
        {
            int lx = startIdx & 63;
            int ly = startIdx >> 6;

            const int MaxSteps = 256;

            for (int step = 0; step < MaxSteps; step++)
            {
                int idx = (ly << 6) + lx;

                if (Height[idx] <= SeaLevel)
                {
                    break;
                }

                RiverMask[idx] = 1;

                byte hC = Height[idx];

                int bestNx = lx;
                int bestNy = ly;
                byte bestH = hC;

                Consider(lx, ly - 1, ref bestNx, ref bestNy, ref bestH, ref rng);
                Consider(lx, ly + 1, ref bestNx, ref bestNy, ref bestH, ref rng);
                Consider(lx + 1, ly, ref bestNx, ref bestNy, ref bestH, ref rng);
                Consider(lx - 1, ly, ref bestNx, ref bestNy, ref bestH, ref rng);

                if (bestNx == lx && bestNy == ly)
                {
                    break;
                }

                lx = bestNx;
                ly = bestNy;
            }
        }

        private void Consider(int nx, int ny, ref int bestNx, ref int bestNy, ref byte bestH, ref SplitMix64 rng)
        {
            if ((uint)nx >= 64u || (uint)ny >= 64u)
            {
                return;
            }

            int nidx = (ny << 6) + nx;
            byte h = Height[nidx];

            if (h < bestH)
            {
                bestH = h;
                bestNx = nx;
                bestNy = ny;
            }
            else if (h == bestH)
            {
                if ((rng.NextU32() & 1u) == 0u)
                {
                    bestNx = nx;
                    bestNy = ny;
                }
            }
        }
    }
}
