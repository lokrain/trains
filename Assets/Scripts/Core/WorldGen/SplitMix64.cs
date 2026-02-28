#nullable enable
using System.Runtime.CompilerServices;

namespace OpenTTD.Core.WorldGen
{
    /// <summary>
    /// Deterministic SplitMix64 pseudo-random generator used for seeded worldgen steps.
    /// </summary>
    public struct SplitMix64
    {
        private ulong _state;

        /// <summary>
        /// Creates a new SplitMix64 generator with the given seed.
        /// </summary>
        /// <param name="seed">Initial generator state.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SplitMix64(ulong seed)
        {
            _state = seed;
        }

        /// <summary>
        /// Returns the next 64-bit pseudo-random value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextU64()
        {
            ulong z = (_state += 0x9E3779B97F4A7C15ul);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9ul;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBul;
            return z ^ (z >> 31);
        }

        /// <summary>
        /// Returns the next 32-bit pseudo-random value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextU32()
        {
            return (uint)NextU64();
        }

        /// <summary>
        /// Returns the next 8-bit pseudo-random value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte NextU8()
        {
            return (byte)NextU64();
        }
    }
}
