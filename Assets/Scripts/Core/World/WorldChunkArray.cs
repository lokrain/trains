#nullable enable
using System;
using Unity.Collections;

namespace OpenTTD.Core.World
{
    /// <summary>
    /// Fixed 32x32 chunk array with direct index access. Owns all chunk allocations.
    /// </summary>
    public struct WorldChunkArray : IDisposable
    {
        /// <summary>
        /// Fixed world chunk storage (length = 1024 for 32x32).
        /// </summary>
        public NativeArray<ChunkSoA> Chunks;

        /// <summary>
        /// World sea level used for derived sea rule.
        /// </summary>
        public byte SeaLevel;

        /// <summary>
        /// Returns true when chunk array storage is initialized.
        /// </summary>
        public bool IsCreated => Chunks.IsCreated;

        /// <summary>
        /// Creates a world chunk array and initializes all chunk SoA allocations.
        /// </summary>
        /// <param name="seaLevel">Sea level threshold.</param>
        /// <param name="allocator">Allocator for the top-level chunk array.</param>
        /// <param name="chunkAllocator">Allocator for each chunk's internal arrays.</param>
        /// <returns>Initialized world chunk array.</returns>
        public static WorldChunkArray Create(byte seaLevel, Allocator allocator, Allocator chunkAllocator)
        {
            WorldChunkArray array = new WorldChunkArray
            {
                Chunks = new NativeArray<ChunkSoA>(WorldConstants.ChunkCount, allocator, NativeArrayOptions.UninitializedMemory),
                SeaLevel = seaLevel
            };

            for (int i = 0; i < array.Chunks.Length; i++)
            {
                array.Chunks[i] = ChunkSoA.Create(chunkAllocator);
            }

            return array;
        }

        /// <summary>
        /// Gets a copy of a chunk by chunk coordinate.
        /// </summary>
        /// <param name="cx">Chunk X coordinate.</param>
        /// <param name="cy">Chunk Y coordinate.</param>
        /// <returns>Chunk value copy.</returns>
        public ChunkSoA GetChunk(int cx, int cy)
        {
            int idx = WorldConstants.ChunkIndex(cx, cy);
            return Chunks[idx];
        }

        /// <summary>
        /// Gets a copy of a chunk by linear chunk index.
        /// </summary>
        /// <param name="chunkIndex">Linear chunk index.</param>
        /// <returns>Chunk value copy.</returns>
        public ChunkSoA GetChunk(int chunkIndex)
        {
            return Chunks[chunkIndex];
        }

        /// <summary>
        /// Sets a chunk value by chunk coordinate.
        /// </summary>
        /// <param name="cx">Chunk X coordinate.</param>
        /// <param name="cy">Chunk Y coordinate.</param>
        /// <param name="chunk">Chunk value to set.</param>
        public void SetChunk(int cx, int cy, ChunkSoA chunk)
        {
            int idx = WorldConstants.ChunkIndex(cx, cy);
            Chunks[idx] = chunk;
        }

        /// <summary>
        /// Sets a chunk value by linear chunk index.
        /// </summary>
        /// <param name="chunkIndex">Linear chunk index.</param>
        /// <param name="chunk">Chunk value to set.</param>
        public void SetChunk(int chunkIndex, ChunkSoA chunk)
        {
            Chunks[chunkIndex] = chunk;
        }

        /// <summary>
        /// Disposes all chunk allocations and top-level storage.
        /// </summary>
        public void Dispose()
        {
            if (!Chunks.IsCreated)
            {
                return;
            }

            for (int i = 0; i < Chunks.Length; i++)
            {
                ChunkSoA chunk = Chunks[i];
                chunk.Dispose();
            }

            Chunks.Dispose();
        }
    }
}
