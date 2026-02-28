#nullable enable
using System;
using System.Buffers;

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Per-(chunk,snapshot) reassembly state.
    /// Managed but pooled; no per-fragment allocations.
    /// </summary>
    public sealed class ReassemblyBuffer : IDisposable
    {
        /// <summary>
        /// Total expected reconstructed payload length.
        /// </summary>
        public readonly int TotalLen;

        /// <summary>
        /// Expected fragment count.
        /// </summary>
        public readonly int FragCount;

        private readonly byte[] _buf;
        private readonly bool[] _got;
        private int _received;

        /// <summary>
        /// Creates a reassembly buffer for one snapshot payload.
        /// </summary>
        /// <param name="totalLen">Total reconstructed payload length.</param>
        /// <param name="fragCount">Expected fragment count.</param>
        public ReassemblyBuffer(int totalLen, int fragCount)
        {
            TotalLen = totalLen;
            FragCount = fragCount;
            _buf = ArrayPool<byte>.Shared.Rent(totalLen);
            _got = ArrayPool<bool>.Shared.Rent(fragCount);
            Array.Clear(_got, 0, fragCount);
            _received = 0;
        }

        /// <summary>
        /// Adds a fragment payload at a known offset.
        /// Duplicate fragments are accepted and ignored.
        /// </summary>
        /// <param name="fragIndex">Fragment index in [0..FragCount-1].</param>
        /// <param name="payload">Fragment payload bytes.</param>
        /// <param name="fragOffset">Destination offset in reassembly buffer.</param>
        /// <returns>True if fragment is valid and accepted.</returns>
        public bool TryAdd(int fragIndex, ReadOnlySpan<byte> payload, int fragOffset)
        {
            if ((uint)fragIndex >= (uint)FragCount)
            {
                return false;
            }

            if (fragOffset < 0)
            {
                return false;
            }

            if (fragOffset + payload.Length > TotalLen)
            {
                return false;
            }

            if (_got[fragIndex])
            {
                return true;
            }

            payload.CopyTo(_buf.AsSpan(fragOffset, payload.Length));
            _got[fragIndex] = true;
            _received++;
            return true;
        }

        /// <summary>
        /// Returns true when all fragments were received.
        /// </summary>
        public bool IsComplete => _received == FragCount;

        /// <summary>
        /// Returns a contiguous span over the reconstructed payload.
        /// </summary>
        public ReadOnlySpan<byte> AsSpan()
        {
            return _buf.AsSpan(0, TotalLen);
        }

        /// <summary>
        /// Returns pooled arrays to their shared pools.
        /// </summary>
        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(_buf);
            ArrayPool<bool>.Shared.Return(_got);
        }
    }
}
