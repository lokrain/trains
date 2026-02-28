#nullable enable
using Unity.Collections;
using Unity.Mathematics;
using OpenTTD.Core.World;

namespace OpenTTD.Core.WorldGen
{
    /// <summary>
    /// Global river planning:
    /// - plans river sources globally
    /// - routes across full map with deterministic downhill greedy logic
    /// - stamps river tiles into per-chunk river masks
    /// </summary>
    public static class GlobalRiverPlanner
    {
        /// <summary>
        /// Configuration for global river generation.
        /// </summary>
        public struct RiverConfig
        {
            public int RiverCount;
            public int MaxSteps;
            public byte MinSourceHeightAboveSea;
        }

        /// <summary>
        /// Plans global rivers and stamps them into chunk river masks.
        /// </summary>
        public static void PlanAndStamp(
            ref WorldChunkArray world,
            ulong worldSeed,
            RiverConfig cfg,
            Allocator scratchAlloc)
        {
            ulong seed = Hash64.DeriveStageSeed(worldSeed, "rivers_global");

            var best = new NativeList<int2>(cfg.RiverCount * 4, scratchAlloc);

            int samples = cfg.RiverCount * 256;
            for (int i = 0; i < samples; i++)
            {
                ulong h = Hash64.Hash(seed, (ulong)(uint)i, 0xA5A5A5A5ul);
                ushort x = (ushort)(h & 2047);
                ushort y = (ushort)((h >> 11) & 2047);

                byte ht = GetHeight(ref world, x, y);
                if (ht < (byte)(world.SeaLevel + cfg.MinSourceHeightAboveSea))
                {
                    continue;
                }

                best.Add(new int2(x, y));
            }

            if (best.Length == 0)
            {
                best.Dispose();
                return;
            }

            for (int r = 0; r < cfg.RiverCount; r++)
            {
                int bestIdx = -1;
                int bestScore = int.MinValue;

                for (int i = 0; i < best.Length; i++)
                {
                    int2 p = best[i];
                    byte ht = GetHeight(ref world, (ushort)p.x, (ushort)p.y);

                    int jitter = (int)(Hash64.Hash(seed, (ulong)(uint)r, (ulong)((uint)p.x | ((uint)p.y << 16))) & 1023u);
                    int score = ht * 1024 + jitter;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestIdx = i;
                    }
                }

                if (bestIdx < 0)
                {
                    break;
                }

                int2 src = best[bestIdx];
                RouteSingleRiver(ref world, seed, riverIndex: r, (ushort)src.x, (ushort)src.y, cfg.MaxSteps);

                best[bestIdx] = best[best.Length - 1];
                best.RemoveAt(best.Length - 1);

                if (best.Length == 0)
                {
                    break;
                }
            }

            best.Dispose();
        }

        private static void RouteSingleRiver(ref WorldChunkArray world, ulong seed, int riverIndex, ushort sx, ushort sy, int maxSteps)
        {
            int x = sx;
            int y = sy;

            for (int step = 0; step < maxSteps; step++)
            {
                byte hC = GetHeight(ref world, (ushort)x, (ushort)y);
                if (hC <= world.SeaLevel)
                {
                    break;
                }

                StampRiver(ref world, (ushort)x, (ushort)y);

                int2 best = new int2(x, y);
                byte bestH = hC;

                Consider(ref world, seed, riverIndex, step, x, y, x, y - 1, ref best, ref bestH);
                Consider(ref world, seed, riverIndex, step, x, y, x, y + 1, ref best, ref bestH);
                Consider(ref world, seed, riverIndex, step, x, y, x + 1, y, ref best, ref bestH);
                Consider(ref world, seed, riverIndex, step, x, y, x - 1, y, ref best, ref bestH);

                if (best.x == x && best.y == y)
                {
                    break;
                }

                x = best.x;
                y = best.y;
            }
        }

        private static void Consider(
            ref WorldChunkArray world,
            ulong seed,
            int riverIndex,
            int step,
            int x0,
            int y0,
            int nx,
            int ny,
            ref int2 best,
            ref byte bestH)
        {
            if ((uint)nx >= WorldConstants.MapW || (uint)ny >= WorldConstants.MapH)
            {
                return;
            }

            byte h = GetHeight(ref world, (ushort)nx, (ushort)ny);
            if (h < bestH)
            {
                bestH = h;
                best = new int2(nx, ny);
            }
            else if (h == bestH)
            {
                ulong t = Hash64.Hash(
                    seed,
                    (ulong)(uint)riverIndex,
                    (ulong)((uint)step ^ ((uint)nx << 11) ^ ((uint)ny << 22)));

                ulong cur = Hash64.Hash(
                    seed,
                    (ulong)(uint)riverIndex,
                    (ulong)((uint)step ^ ((uint)best.x << 11) ^ ((uint)best.y << 22)));

                if (t < cur)
                {
                    best = new int2(nx, ny);
                }
            }
        }

        private static byte GetHeight(ref WorldChunkArray world, ushort x, ushort y)
        {
            TileAccessor.WorldToChunkLocal(x, y, out int cx, out int cy, out int lx, out int ly);
            ChunkSoA c = world.GetChunk(cx, cy);
            return c.Height[WorldConstants.TileIndex(lx, ly)];
        }

        private static void StampRiver(ref WorldChunkArray world, ushort x, ushort y)
        {
            TileAccessor.WorldToChunkLocal(x, y, out int cx, out int cy, out int lx, out int ly);
            ChunkSoA c = world.GetChunk(cx, cy);
            c.RiverMask[WorldConstants.TileIndex(lx, ly)] = 1;
            world.SetChunk(cx, cy, c);
        }
    }
}
