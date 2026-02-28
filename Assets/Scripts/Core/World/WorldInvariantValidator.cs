#nullable enable

namespace OpenTTD.Core.World
{
    /// <summary>
    /// Fast world consistency checks for derived fields and masks.
    /// Intended for debug/test validation paths.
    /// </summary>
    public static class WorldInvariantValidator
    {
        /// <summary>
        /// Validates one chunk for slope/build-mask/sea consistency.
        /// </summary>
        public static bool ValidateChunk(ref WorldChunkArray world, int chunkX, int chunkY)
        {
            ChunkSoA chunk = world.GetChunk(chunkX, chunkY);
            int n = chunk.Height.Length;

            for (int i = 0; i < n; i++)
            {
                byte slope = chunk.Slope[i];
                if (slope > 3)
                {
                    return false;
                }

                bool isRiver = chunk.RiverMask[i] != 0;
                bool isSea = !isRiver && chunk.Height[i] <= world.SeaLevel;
                ushort mask = chunk.BuildMask[i];

                if (((mask & BuildMaskBits.IsSea) != 0) != isSea)
                {
                    return false;
                }

                if (((mask & BuildMaskBits.IsRiver) != 0) != isRiver)
                {
                    return false;
                }

                ushort encodedSlope = (ushort)(mask >> BuildMaskBits.SlopeClassShift);
                if (encodedSlope != slope)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates all chunks in world.
        /// </summary>
        public static bool ValidateWorld(ref WorldChunkArray world)
        {
            for (int chunkIndex = 0; chunkIndex < WorldConstants.ChunkCount; chunkIndex++)
            {
                ChunkCoord cc = ChunkCoord.FromIndex(chunkIndex);
                if (!ValidateChunk(ref world, cc.X, cc.Y))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
