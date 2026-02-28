#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace OpenTTD.Infra.Determinism
{
    /// <summary>
    /// Canonical deterministic hash combiner scaffold.
    /// </summary>
    public static class DeterministicHashing
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Seed(ulong v)
        {
            return Mix(v ^ 0xCBF29CE484222325ul);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Combine(ulong current, ulong value)
        {
            return Mix(current ^ (value + 0x9E3779B97F4A7C15ul + (current << 6) + (current >> 2)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Combine(ulong current, uint value)
        {
            return Combine(current, (ulong)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Mix(ulong x)
        {
            x ^= x >> 30;
            x *= 0xBF58476D1CE4E5B9ul;
            x ^= x >> 27;
            x *= 0x94D049BB133111EBul;
            x ^= x >> 31;
            return x;
        }
    }

    /// <summary>
    /// Optional interface for deterministic state hash contributors.
    /// </summary>
    public interface IDeterministicHashContributor
    {
        void Contribute(ref ulong hash);
    }

    /// <summary>
    /// Snapshot hash record used by replay/determinism harnesses.
    /// </summary>
    public readonly struct HashSample : IEquatable<HashSample>
    {
        public readonly ulong Tick;
        public readonly ulong Hash;

        public HashSample(ulong tick, ulong hash)
        {
            Tick = tick;
            Hash = hash;
        }

        public bool Equals(HashSample other)
        {
            return Tick == other.Tick && Hash == other.Hash;
        }

        public override bool Equals(object obj)
        {
            return obj is HashSample other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Tick, Hash);
        }
    }
}
