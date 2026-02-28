#nullable enable
using System;
using Unity.Collections;
using OpenTTD.Core.World;

namespace OpenTTD.Core.WorldGen
{
    /// <summary>
    /// Deterministic world generation orchestration entrypoint.
    /// Stages: config parse/validate, height pass, global rivers, biome pass, derived recompute.
    /// </summary>
    public static class WorldGenOrchestrator
    {
        public static bool TryGenerateWorld(
            ReadOnlySpan<byte> configBlob,
            out WorldGenConfig config,
            out WorldChunkArray world,
            out WorldGenConfigError error,
            int chunksPerBatch = 8)
        {
            config = default;
            world = default;
            error = WorldGenConfigError.None;

            if (configBlob.Length < WorldGenConfigBlob.SizeBytes)
            {
                error = WorldGenConfigError.UnsupportedVersion;
                return false;
            }

            config = WorldGenConfigBlob.Read(configBlob);
            if (!WorldGenConfigValidation.TryValidate(config, out error))
            {
                return false;
            }

            world = GenerateWorld(config, chunksPerBatch);
            return true;
        }

        public static WorldChunkArray GenerateWorld(in WorldGenConfig config, int chunksPerBatch = 8)
        {
            WorldChunkArray world = WorldChunkArray.Create(config.SeaLevel, Allocator.Persistent, Allocator.Persistent);

            int batch = chunksPerBatch <= 0 ? 1 : chunksPerBatch;
            var scheduler = new WorldGenScheduler(config, Allocator.Temp);
            for (int i = 0; i < WorldConstants.ChunkCount; i += batch)
            {
                scheduler.ScheduleNextBatch(ref world, batch);
            }

            scheduler.CompleteAll();
            scheduler.Dispose();

            GlobalRiverPlanner.PlanAndStamp(
                ref world,
                config.WorldSeed,
                new GlobalRiverPlanner.RiverConfig
                {
                    RiverCount = config.RiverCount,
                    MaxSteps = config.RiverMaxSteps,
                    MinSourceHeightAboveSea = config.RiverMinSourceAboveSea
                },
                Allocator.Temp);

            for (int chunkIndex = 0; chunkIndex < WorldConstants.ChunkCount; chunkIndex++)
            {
                ChunkCoord cc = ChunkCoord.FromIndex(chunkIndex);
                ChunkSoA chunk = world.GetChunk(chunkIndex);

                var biomeJob = new BiomeGenJob
                {
                    ChunkX = cc.X,
                    ChunkY = cc.Y,
                    SeaLevel = config.SeaLevel,
                    Height = chunk.Height,
                    RiverMask = chunk.RiverMask,
                    OutBiome = chunk.Biome
                };

                for (int tileIndex = 0; tileIndex < chunk.Height.Length; tileIndex++)
                {
                    biomeJob.Execute(tileIndex);
                }

                var derivedJob = new DerivedSlopeBuildJob
                {
                    ChunkX = cc.X,
                    ChunkY = cc.Y,
                    SeaLevel = config.SeaLevel,
                    Config = config,
                    World = world,
                    OutSlope = chunk.Slope,
                    OutBuildMask = chunk.BuildMask,
                    RiverMask = chunk.RiverMask
                };

                for (int tileIndex = 0; tileIndex < chunk.Height.Length; tileIndex++)
                {
                    derivedJob.Execute(tileIndex);
                }

                chunk.Dirty = ChunkDirtyFlags.None;
                chunk.ClearDirtyRect();
                chunk.Versions.HeightVersion = 1;
                chunk.Versions.DerivedVersion = 1;
                chunk.Versions.SnapshotVersion = 1;
                chunk.Versions.RenderVersion = 1;
                world.SetChunk(chunkIndex, chunk);
            }

            return world;
        }
    }
}
