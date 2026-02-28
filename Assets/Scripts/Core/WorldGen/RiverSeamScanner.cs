#nullable enable
using OpenTTD.Core.World;

namespace OpenTTD.Core.WorldGen
{
    /// <summary>
    /// Chunk-border river continuity scanner.
    /// Reports mismatches for adjacent edge tiles across chunk seams.
    /// </summary>
    public static class RiverSeamScanner
    {
        public readonly struct RiverSeamReport
        {
            public readonly int SeamChecks;
            public readonly int SeamMismatches;
            public readonly int FirstMismatchChunkX;
            public readonly int FirstMismatchChunkY;
            public readonly int FirstMismatchLocal;
            public readonly bool IsHorizontalSeam;

            public RiverSeamReport(
                int seamChecks,
                int seamMismatches,
                int firstMismatchChunkX,
                int firstMismatchChunkY,
                int firstMismatchLocal,
                bool isHorizontalSeam)
            {
                SeamChecks = seamChecks;
                SeamMismatches = seamMismatches;
                FirstMismatchChunkX = firstMismatchChunkX;
                FirstMismatchChunkY = firstMismatchChunkY;
                FirstMismatchLocal = firstMismatchLocal;
                IsHorizontalSeam = isHorizontalSeam;
            }

            public bool IsClean => SeamMismatches == 0;
        }

        public static RiverSeamReport Scan(ref WorldChunkArray world)
        {
            int checks = 0;
            int mismatches = 0;
            int firstCx = -1;
            int firstCy = -1;
            int firstLocal = -1;
            bool firstHorizontal = false;

            // Vertical seams between chunk x and x+1.
            for (int cy = 0; cy < WorldConstants.ChunksH; cy++)
            {
                for (int cx = 0; cx < WorldConstants.ChunksW - 1; cx++)
                {
                    ChunkSoA left = world.GetChunk(cx, cy);
                    ChunkSoA right = world.GetChunk(cx + 1, cy);

                    for (int ly = 0; ly < WorldConstants.ChunkSize; ly++)
                    {
                        int leftIdx = WorldConstants.TileIndex(WorldConstants.ChunkSize - 1, ly);
                        int rightIdx = WorldConstants.TileIndex(0, ly);
                        bool l = left.RiverMask[leftIdx] != 0;
                        bool r = right.RiverMask[rightIdx] != 0;
                        checks++;
                        if (l != r)
                        {
                            mismatches++;
                            if (firstCx < 0)
                            {
                                firstCx = cx;
                                firstCy = cy;
                                firstLocal = ly;
                                firstHorizontal = false;
                            }
                        }
                    }
                }
            }

            // Horizontal seams between chunk y and y+1.
            for (int cy = 0; cy < WorldConstants.ChunksH - 1; cy++)
            {
                for (int cx = 0; cx < WorldConstants.ChunksW; cx++)
                {
                    ChunkSoA bottom = world.GetChunk(cx, cy);
                    ChunkSoA top = world.GetChunk(cx, cy + 1);

                    for (int lx = 0; lx < WorldConstants.ChunkSize; lx++)
                    {
                        int bottomIdx = WorldConstants.TileIndex(lx, WorldConstants.ChunkSize - 1);
                        int topIdx = WorldConstants.TileIndex(lx, 0);
                        bool b = bottom.RiverMask[bottomIdx] != 0;
                        bool t = top.RiverMask[topIdx] != 0;
                        checks++;
                        if (b != t)
                        {
                            mismatches++;
                            if (firstCx < 0)
                            {
                                firstCx = cx;
                                firstCy = cy;
                                firstLocal = lx;
                                firstHorizontal = true;
                            }
                        }
                    }
                }
            }

            return new RiverSeamReport(checks, mismatches, firstCx, firstCy, firstLocal, firstHorizontal);
        }
    }
}
