#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace OpenTTD.Infra.Determinism
{
    /// <summary>
    /// Deterministic RNG stream derivation and stream-local number generation.
    /// </summary>
    public static class DeterministicRngStreams
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong DeriveSubstreamSeed(ulong baseSeed, ulong streamTag)
        {
            return Mix(baseSeed ^ (streamTag * 0x9E3779B97F4A7C15ul));
        }

        public static ulong DeriveSubstreamSeed(ulong baseSeed, string streamName)
        {
            return DeriveSubstreamSeed(baseSeed, Tag(streamName));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DeterministicRngStream CreateStream(ulong baseSeed, ulong streamTag)
        {
            return new DeterministicRngStream(DeriveSubstreamSeed(baseSeed, streamTag));
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

        private static ulong Tag(string value)
        {
            unchecked
            {
                ulong h = 1469598103934665603ul;
                for (int i = 0; i < value.Length; i++)
                {
                    h ^= value[i];
                    h *= 1099511628211ul;
                }

                return h;
            }
        }
    }

    /// <summary>
    /// Deterministic substream RNG with explicit counter state.
    /// </summary>
    public struct DeterministicRngStream : IEquatable<DeterministicRngStream>
    {
        private readonly ulong _seed;
        private ulong _counter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DeterministicRngStream(ulong seed)
        {
            _seed = seed;
            _counter = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextU64()
        {
            ulong x = _seed ^ (_counter * 0x9E3779B97F4A7C15ul);
            _counter += 1;
            return DeterministicRngStreams.Mix(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextU32()
        {
            return (uint)NextU64();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte NextU8()
        {
            return (byte)NextU64();
        }

        public readonly bool Equals(DeterministicRngStream other)
        {
            return _seed == other._seed && _counter == other._counter;
        }

        public override bool Equals(object obj)
        {
            return obj is DeterministicRngStream other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_seed, _counter);
        }
    }
}
