#nullable enable
using System.Runtime.CompilerServices;

namespace OpenTTD.Core.WorldGen
{
    /// <summary>
    /// 64-bit deterministic hashing helpers for world generation seed derivation.
    /// </summary>
    public static class Hash64
    {
        /// <summary>
        /// SplitMix64 finalizer with strong avalanche behavior.
        /// </summary>
        /// <param name="x">Input value.</param>
        /// <returns>Mixed 64-bit hash value.</returns>
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

        /// <summary>
        /// Hashes two inputs plus seed into a 64-bit value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Hash(ulong seed, ulong a, ulong b)
        {
            return Mix(seed ^ (a * 0x9E3779B97F4A7C15ul) ^ (b * 0xC2B2AE3D27D4EB4Ful));
        }

        /// <summary>
        /// Hashes one input plus seed into a 64-bit value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Hash(ulong seed, ulong a)
        {
            return Mix(seed ^ (a * 0x9E3779B97F4A7C15ul));
        }

        /// <summary>
        /// FNV-1a style tag hash for stage derivation.
        /// </summary>
        /// <param name="tag">Stage tag text.</param>
        /// <returns>Deterministic 64-bit tag hash.</returns>
        public static ulong Tag(string tag)
        {
            unchecked
            {
                ulong h = 1469598103934665603ul;
                for (int i = 0; i < tag.Length; i++)
                {
                    h ^= tag[i];
                    h *= 1099511628211ul;
                }

                return h;
            }
        }

        /// <summary>
        /// Derives a deterministic stage seed from world seed and stage tag.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong DeriveStageSeed(ulong worldSeed, string tag)
        {
            return Mix(worldSeed ^ Tag(tag));
        }
    }
}
