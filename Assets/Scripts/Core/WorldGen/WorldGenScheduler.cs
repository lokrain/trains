#nullable enable
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using OpenTTD.Core.World;

namespace OpenTTD.Core.WorldGen
{
    /// <summary>
    /// Async chunk generation scheduler.
    /// Design:
    /// - server schedules chunk jobs in waves and can accept connections early
    /// - clients stream chunks as they become ready
    ///
    /// This type handles scheduling only; networking/streaming is separate.
    /// </summary>
    public struct WorldGenScheduler : IDisposable
    {
        public readonly ulong WorldSeed;
        public readonly byte SeaLevel;

        private NativeQueue<int> _pendingChunks;
        private NativeQueue<int> _completedChunks;

        private JobHandle _currentBatch;

        /// <summary>
        /// Returns true when scheduler native storage is initialized.
        /// </summary>
        public bool IsCreated => _pendingChunks.IsCreated;

        /// <summary>
        /// Creates a scheduler and enqueues all chunks for generation.
        /// </summary>
        public WorldGenScheduler(ulong worldSeed, byte seaLevel, Allocator alloc)
        {
            WorldSeed = worldSeed;
            SeaLevel = seaLevel;
            _pendingChunks = new NativeQueue<int>(alloc);
            _completedChunks = new NativeQueue<int>(alloc);
            _currentBatch = default;

            for (int i = 0; i < WorldConstants.ChunkCount; i++)
            {
                _pendingChunks.Enqueue(i);
            }
        }

        /// <summary>
        /// Releases native resources owned by the scheduler.
        /// </summary>
        public void Dispose()
        {
            _currentBatch.Complete();

            if (_pendingChunks.IsCreated)
            {
                _pendingChunks.Dispose();
            }

            if (_completedChunks.IsCreated)
            {
                _completedChunks.Dispose();
            }
        }

        /// <summary>
        /// Tries to dequeue a chunk index that completed generation.
        /// </summary>
        public bool TryDequeueCompleted(out int chunkIndex)
        {
            if (_completedChunks.Count > 0)
            {
                chunkIndex = _completedChunks.Dequeue();
                return true;
            }

            chunkIndex = default;
            return false;
        }

        /// <summary>
        /// Schedules the next wave of chunk generation jobs.
        /// </summary>
        /// <param name="world">World chunk array storage.</param>
        /// <param name="maxChunksThisBatch">Maximum chunks to schedule in this wave.</param>
        public void ScheduleNextBatch(ref WorldChunkArray world, int maxChunksThisBatch = 8)
        {
            _currentBatch.Complete();

            if (_pendingChunks.Count == 0)
            {
                return;
            }

            int n = maxChunksThisBatch;
            JobHandle combined = default;

            ulong seedHeight = Hash64.DeriveStageSeed(WorldSeed, "height");
            ulong seedRivers = Hash64.DeriveStageSeed(WorldSeed, "rivers");

            for (int i = 0; i < n && _pendingChunks.Count > 0; i++)
            {
                int chunkIndex = _pendingChunks.Dequeue();
                ChunkCoord cc = ChunkCoord.FromIndex(chunkIndex);

                ChunkSoA c = world.GetChunk(chunkIndex);

                JobHandle hJob = new HeightFieldGenJob
                {
                    SeedHeight = seedHeight,
                    ChunkX = cc.X,
                    ChunkY = cc.Y,
                    OutHeight = c.Height
                }.Schedule(c.Height.Length, 128, combined);

                JobHandle rJob = new RiverGenJob
                {
                    SeedRivers = seedRivers,
                    ChunkX = cc.X,
                    ChunkY = cc.Y,
                    SeaLevel = world.SeaLevel,
                    Height = c.Height,
                    RiverMask = c.RiverMask,
                    RiversPerChunk = 1
                }.Schedule(hJob);

                JobHandle bJob = new BiomeGenJob
                {
                    ChunkX = cc.X,
                    ChunkY = cc.Y,
                    SeaLevel = world.SeaLevel,
                    Height = c.Height,
                    RiverMask = c.RiverMask,
                    OutBiome = c.Biome
                }.Schedule(c.Height.Length, 128, rJob);

                JobHandle dJob = new DerivedSlopeBuildJob
                {
                    ChunkX = cc.X,
                    ChunkY = cc.Y,
                    SeaLevel = world.SeaLevel,
                    World = world,
                    OutSlope = c.Slope,
                    OutBuildMask = c.BuildMask,
                    RiverMask = c.RiverMask
                }.Schedule(c.Height.Length, 128, bJob);

                JobHandle doneJob = new EnqueueCompletedChunkJob
                {
                    ChunkIndex = chunkIndex,
                    Completed = _completedChunks.AsParallelWriter()
                }.Schedule(dJob);

                combined = JobHandle.CombineDependencies(combined, doneJob);
            }

            _currentBatch = combined;
        }

        /// <summary>
        /// Waits until currently scheduled batch completes.
        /// </summary>
        public void CompleteAll()
        {
            _currentBatch.Complete();
        }

        [BurstCompile]
        private struct EnqueueCompletedChunkJob : IJob
        {
            public int ChunkIndex;
            public NativeQueue<int>.ParallelWriter Completed;

            public void Execute()
            {
                Completed.Enqueue(ChunkIndex);
            }
        }
    }
}
