#nullable enable
using System;

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Global world snapshot identity metadata.
    /// </summary>
    public readonly struct WorldSnapshotIdentity : IEquatable<WorldSnapshotIdentity>
    {
        public readonly uint Epoch;
        public readonly ulong WorldSnapshotId;

        public WorldSnapshotIdentity(uint epoch, ulong worldSnapshotId)
        {
            Epoch = epoch;
            WorldSnapshotId = worldSnapshotId;
        }

        public bool Equals(WorldSnapshotIdentity other)
        {
            return Epoch == other.Epoch && WorldSnapshotId == other.WorldSnapshotId;
        }

        public override bool Equals(object obj)
        {
            return obj is WorldSnapshotIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Epoch, WorldSnapshotId);
        }
    }

    /// <summary>
    /// Per-chunk snapshot identity metadata.
    /// </summary>
    public readonly struct ChunkSnapshotIdentity : IEquatable<ChunkSnapshotIdentity>
    {
        public readonly uint Epoch;
        public readonly short ChunkX;
        public readonly short ChunkY;
        public readonly ulong ChunkSnapshotId;

        public ChunkSnapshotIdentity(uint epoch, short chunkX, short chunkY, ulong chunkSnapshotId)
        {
            Epoch = epoch;
            ChunkX = chunkX;
            ChunkY = chunkY;
            ChunkSnapshotId = chunkSnapshotId;
        }

        public bool Equals(ChunkSnapshotIdentity other)
        {
            return Epoch == other.Epoch
                && ChunkX == other.ChunkX
                && ChunkY == other.ChunkY
                && ChunkSnapshotId == other.ChunkSnapshotId;
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkSnapshotIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Epoch, ChunkX, ChunkY, ChunkSnapshotId);
        }
    }
}
