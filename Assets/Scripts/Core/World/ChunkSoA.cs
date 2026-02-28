#nullable enable
using System;
using Unity.Collections;

namespace OpenTTD.Core.World
{
    /// <summary>
    /// Authoritative chunk data stored as Structure-of-Arrays.
    /// Sea is derived from height + sea level + river mask.
    /// Rivers are static via river mask.
    /// Biomes are static.
    /// </summary>
    public struct ChunkSoA : IDisposable
    {
        /// <summary>
        /// Tile height data [4096].
        /// </summary>
        public NativeArray<byte> Height;

        /// <summary>
        /// Static river mask [4096], 0/1 values.
        /// </summary>
        public NativeArray<byte> RiverMask;

        /// <summary>
        /// Static biome ids [4096].
        /// </summary>
        public NativeArray<byte> Biome;

        /// <summary>
        /// Derived slope classes [4096].
        /// </summary>
        public NativeArray<byte> Slope;

        /// <summary>
        /// Derived buildability mask bits [4096].
        /// </summary>
        public NativeArray<ushort> BuildMask;

        /// <summary>
        /// Per-chunk version counters.
        /// </summary>
        public ChunkVersions Versions;

        /// <summary>
        /// Dirty flags driving recompute/snapshot/render scheduling.
        /// </summary>
        public ChunkDirtyFlags Dirty;

        /// <summary>
        /// Returns true when chunk arrays are initialized.
        /// </summary>
        public bool IsCreated => Height.IsCreated;

        /// <summary>
        /// Creates a chunk with fixed-size SoA arrays using the provided allocator.
        /// </summary>
        /// <param name="allocator">Native allocator used for arrays.</param>
        /// <returns>Initialized chunk SoA.</returns>
        public static ChunkSoA Create(Allocator allocator)
        {
            int n = WorldConstants.ChunkSize * WorldConstants.ChunkSize;
            return new ChunkSoA
            {
                Height = new NativeArray<byte>(n, allocator, NativeArrayOptions.UninitializedMemory),
                RiverMask = new NativeArray<byte>(n, allocator, NativeArrayOptions.ClearMemory),
                Biome = new NativeArray<byte>(n, allocator, NativeArrayOptions.ClearMemory),
                Slope = new NativeArray<byte>(n, allocator, NativeArrayOptions.UninitializedMemory),
                BuildMask = new NativeArray<ushort>(n, allocator, NativeArrayOptions.UninitializedMemory),
                Versions = default,
                Dirty = ChunkDirtyFlags.None
            };
        }

        /// <summary>
        /// Disposes all native arrays owned by this chunk.
        /// </summary>
        public void Dispose()
        {
            if (Height.IsCreated)
            {
                Height.Dispose();
            }

            if (RiverMask.IsCreated)
            {
                RiverMask.Dispose();
            }

            if (Biome.IsCreated)
            {
                Biome.Dispose();
            }

            if (Slope.IsCreated)
            {
                Slope.Dispose();
            }

            if (BuildMask.IsCreated)
            {
                BuildMask.Dispose();
            }
        }
    }
}
