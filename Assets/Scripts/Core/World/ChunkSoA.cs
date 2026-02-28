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
        /// Dirty rect local min X (0..63), 255 when not initialized.
        /// </summary>
        public byte DirtyMinX;

        /// <summary>
        /// Dirty rect local min Y (0..63), 255 when not initialized.
        /// </summary>
        public byte DirtyMinY;

        /// <summary>
        /// Dirty rect local max X (0..63).
        /// </summary>
        public byte DirtyMaxX;

        /// <summary>
        /// Dirty rect local max Y (0..63).
        /// </summary>
        public byte DirtyMaxY;

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
                Dirty = ChunkDirtyFlags.None,
                DirtyMinX = byte.MaxValue,
                DirtyMinY = byte.MaxValue,
                DirtyMaxX = 0,
                DirtyMaxY = 0
            };
        }

        /// <summary>
        /// Adds a local tile position to dirty rect accumulator.
        /// </summary>
        public void MarkDirtyTile(byte lx, byte ly)
        {
            if (DirtyMinX == byte.MaxValue)
            {
                DirtyMinX = lx;
                DirtyMaxX = lx;
                DirtyMinY = ly;
                DirtyMaxY = ly;
                return;
            }

            if (lx < DirtyMinX)
            {
                DirtyMinX = lx;
            }

            if (lx > DirtyMaxX)
            {
                DirtyMaxX = lx;
            }

            if (ly < DirtyMinY)
            {
                DirtyMinY = ly;
            }

            if (ly > DirtyMaxY)
            {
                DirtyMaxY = ly;
            }
        }

        /// <summary>
        /// Clears dirty rect accumulator.
        /// </summary>
        public void ClearDirtyRect()
        {
            DirtyMinX = byte.MaxValue;
            DirtyMinY = byte.MaxValue;
            DirtyMaxX = 0;
            DirtyMaxY = 0;
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
