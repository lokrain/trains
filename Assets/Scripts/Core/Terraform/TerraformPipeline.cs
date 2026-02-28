#nullable enable
using OpenTTD.Core.World;
using OpenTTD.Core.WorldGen;

namespace OpenTTD.Core.Terraform
{
    /// <summary>
    /// Server-authoritative terraform pipeline:
    /// - apply height rect edit
    /// - bump height version
    /// - recompute derived fields for 3x3 dirty ring around edited chunk
    /// - bump derived/snapshot/render versions and flags
    /// </summary>
    public static class TerraformPipeline
    {
        /// <summary>
        /// Applies a chunk-local terraform rect and recomputes derived data in a 3x3 chunk ring.
        /// </summary>
        public static bool ApplyRectAndRecompute3x3(
            ref WorldChunkArray world,
            in WorldGenConfig config,
            int chunkX,
            int chunkY,
            byte rx,
            byte ry,
            byte rw,
            byte rh,
            sbyte delta)
        {
            if ((uint)chunkX >= WorldConstants.ChunksW || (uint)chunkY >= WorldConstants.ChunksH)
            {
                return false;
            }

            if (rw == 0 || rh == 0)
            {
                return false;
            }

            if (rx + rw > WorldConstants.ChunkSize || ry + rh > WorldConstants.ChunkSize)
            {
                return false;
            }

            int editedChunkIndex = WorldConstants.ChunkIndex(chunkX, chunkY);
            ChunkSoA editedChunk = world.GetChunk(editedChunkIndex);

            var applyJob = new ApplyTerraformRectJob
            {
                ChunkX = chunkX,
                ChunkY = chunkY,
                Rx = rx,
                Ry = ry,
                Rw = rw,
                Rh = rh,
                Delta = delta,
                Height = editedChunk.Height
            };

            int rectTiles = applyJob.RectTileCount;
            for (int i = 0; i < rectTiles; i++)
            {
                applyJob.Execute(i);
            }

            editedChunk.Versions.HeightVersion += 1;
            editedChunk.Dirty |= ChunkDirtyFlags.Height;
            world.SetChunk(editedChunkIndex, editedChunk);

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = chunkX + dx;
                    int ny = chunkY + dy;
                    if ((uint)nx >= WorldConstants.ChunksW || (uint)ny >= WorldConstants.ChunksH)
                    {
                        continue;
                    }

                    int chunkIndex = WorldConstants.ChunkIndex(nx, ny);
                    ChunkSoA chunk = world.GetChunk(chunkIndex);

                    var recomputeJob = new RecomputeDerivedForChunkJob
                    {
                        ChunkX = nx,
                        ChunkY = ny,
                        World = world,
                        Config = config,
                        RiverMask = chunk.RiverMask,
                        OutSlope = chunk.Slope,
                        OutBuildMask = chunk.BuildMask
                    };

                    for (int tileIndex = 0; tileIndex < chunk.Slope.Length; tileIndex++)
                    {
                        recomputeJob.Execute(tileIndex);
                    }

                    chunk.Versions.DerivedVersion += 1;
                    chunk.Versions.SnapshotVersion += 1;
                    chunk.Versions.RenderVersion += 1;
                    chunk.Dirty |= ChunkDirtyFlags.Derived | ChunkDirtyFlags.Snapshot | ChunkDirtyFlags.Render;
                    world.SetChunk(chunkIndex, chunk);
                }
            }

            return true;
        }
    }
}
