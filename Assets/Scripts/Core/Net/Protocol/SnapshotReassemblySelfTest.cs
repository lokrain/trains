#nullable enable
using System;

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Minimal self-test for reassembly correctness, duplicate handling, and timeout eviction.
    /// </summary>
    public static class SnapshotReassemblySelfTest
    {
        public static bool Run()
        {
            using var manager = new SnapshotReassemblyManager(timeoutTicks: 5);

            if (!manager.TryGetOrCreate(1UL, totalLen: 6, fragCount: 3, nowTick: 0, out ReassemblyBuffer? buffer) || buffer == null)
            {
                return false;
            }

            if (!buffer.TryAdd(2, new byte[] { (byte)'e', (byte)'f' }, fragOffset: 4))
            {
                return false;
            }

            if (!buffer.TryAdd(0, new byte[] { (byte)'a', (byte)'b' }, fragOffset: 0))
            {
                return false;
            }

            if (!buffer.TryAdd(0, new byte[] { (byte)'x', (byte)'y' }, fragOffset: 0))
            {
                return false;
            }

            if (buffer.IsComplete)
            {
                return false;
            }

            if (buffer.TryAdd(1, new byte[] { (byte)'c', (byte)'d', (byte)'z' }, fragOffset: 2))
            {
                return false;
            }

            if (!buffer.TryAdd(1, new byte[] { (byte)'c', (byte)'d' }, fragOffset: 2))
            {
                return false;
            }

            if (!buffer.IsComplete)
            {
                return false;
            }

            ReadOnlySpan<byte> reconstructed = buffer.AsSpan();
            if (reconstructed.Length != 6
                || reconstructed[0] != (byte)'a'
                || reconstructed[1] != (byte)'b'
                || reconstructed[2] != (byte)'c'
                || reconstructed[3] != (byte)'d'
                || reconstructed[4] != (byte)'e'
                || reconstructed[5] != (byte)'f')
            {
                return false;
            }

            manager.Remove(1UL);

            if (!manager.TryGetOrCreate(2UL, totalLen: 4, fragCount: 2, nowTick: 0, out _))
            {
                return false;
            }

            int evicted = manager.EvictExpired(nowTick: 5);
            return evicted == 1;
        }
    }
}
