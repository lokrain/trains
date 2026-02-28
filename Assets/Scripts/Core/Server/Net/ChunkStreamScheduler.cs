#nullable enable
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using OpenTTD.Core.Net.Protocol;
using OpenTTD.Core.World;

namespace OpenTTD.Core.Server.Net
{
    /// <summary>
    /// Per-connection streaming scheduler (RO lane):
    /// - prioritizes chunks around client focus
    /// - throttles per tick
    /// - supports join-in-progress by streaming snapshots as they become available
    /// </summary>
    public sealed class ChunkStreamScheduler
    {
        private BitSet1024 _have;
        private readonly Queue<int> _high;
        private int2 _focus;
        private readonly int _ringRadius;
        private readonly int _bytesPerTickBudget;
        private readonly int _messagesPerTickBudget;
        private bool _joinReady;
        private readonly ReplicationCounters? _counters;

        /// <summary>
        /// Creates a new chunk stream scheduler.
        /// </summary>
        /// <param name="ringRadius">Priority radius around current focus chunk.</param>
        public ChunkStreamScheduler(
            int ringRadius = 3,
            int bytesPerTickBudget = 32 * 1024,
            int messagesPerTickBudget = 64,
            ReplicationCounters? counters = null)
        {
            _have = new BitSet1024();
            _high = new Queue<int>(64);
            _focus = new int2(0, 0);
            _ringRadius = ringRadius;
            _bytesPerTickBudget = bytesPerTickBudget <= 0 ? 32 * 1024 : bytesPerTickBudget;
            _messagesPerTickBudget = messagesPerTickBudget <= 0 ? 64 : messagesPerTickBudget;
            _joinReady = false;
            _counters = counters;
        }

        public int BytesPerTickBudget => _bytesPerTickBudget;

        public int MessagesPerTickBudget => _messagesPerTickBudget;

        public bool IsJoinReady => _joinReady;

        public void ResetJoinReadyFence()
        {
            _joinReady = false;
        }

        public bool UpdateJoinReadyFence(int requiredRadius)
        {
            if (_joinReady)
            {
                return true;
            }

            _joinReady = IsReadyAroundFocus(requiredRadius);
            return _joinReady;
        }

        /// <summary>
        /// Updates scheduler focus chunk.
        /// </summary>
        public void SetFocusChunk(int cx, int cy)
        {
            _focus = new int2(cx, cy);
        }

        /// <summary>
        /// Returns true if client already has given chunk index.
        /// </summary>
        public bool HasChunk(int chunkIndex)
        {
            return _have.Get(chunkIndex);
        }

        /// <summary>
        /// Marks a chunk as present on client.
        /// </summary>
        public void MarkHave(int chunkIndex)
        {
            _have.Set(chunkIndex);
        }

        /// <summary>
        /// Enqueues a high-priority resync chunk request.
        /// </summary>
        public void EnqueueResync(int chunkIndex)
        {
            _high.Enqueue(chunkIndex);
        }

        /// <summary>
        /// Returns true when all chunks within radius around current focus are already present.
        /// Useful for join-in-progress ready gating.
        /// </summary>
        /// <param name="radius">Chebyshev chunk radius around focus.</param>
        public bool IsReadyAroundFocus(int radius)
        {
            int fx = _focus.x;
            int fy = _focus.y;

            int minX = math.max(0, fx - radius);
            int maxX = math.min(WorldConstants.ChunksW - 1, fx + radius);
            int minY = math.max(0, fy - radius);
            int maxY = math.min(WorldConstants.ChunksH - 1, fy + radius);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int idx = WorldConstants.ChunkIndex(x, y);
                    if (!_have.Get(idx))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Gets next chunk index to send snapshot for.
        /// Returns -1 when none are eligible.
        /// </summary>
        /// <param name="isChunkReady">Chunk readiness predicate.</param>
        public int NextChunkToSend(Func<int, bool> isChunkReady)
        {
            int remainingBytes = int.MaxValue;
            int remainingMessages = int.MaxValue;
            return NextChunkToSendBudgeted(isChunkReady, static _ => 0, ref remainingBytes, ref remainingMessages);
        }

        /// <summary>
        /// Gets next chunk index to send snapshot for, honoring per-tick byte/message budgets.
        /// Returns -1 when none are eligible under current remaining budget.
        /// </summary>
        /// <param name="isChunkReady">Chunk readiness predicate.</param>
        /// <param name="estimateChunkBytes">Estimated bytes for sending this chunk.</param>
        /// <param name="remainingBytes">Remaining bytes budget for this tick.</param>
        /// <param name="remainingMessages">Remaining message budget for this tick.</param>
        public int NextChunkToSendBudgeted(
            Func<int, bool> isChunkReady,
            Func<int, int> estimateChunkBytes,
            ref int remainingBytes,
            ref int remainingMessages)
        {
            if (remainingMessages <= 0)
            {
                return -1;
            }

            while (_high.Count > 0)
            {
                int idx = _high.Dequeue();
                if (isChunkReady(idx))
                {
                    int estimatedBytes = estimateChunkBytes(idx);
                    if (estimatedBytes > remainingBytes)
                    {
                        _counters?.IncrementSchedulerBudgetDrops();
                        continue;
                    }

                    remainingBytes -= estimatedBytes;
                    remainingMessages--;
                    return idx;
                }
            }

            int fx = _focus.x;
            int fy = _focus.y;

            for (int r = 0; r <= _ringRadius; r++)
            {
                int minX = math.max(0, fx - r);
                int maxX = math.min(WorldConstants.ChunksW - 1, fx + r);
                int minY = math.max(0, fy - r);
                int maxY = math.min(WorldConstants.ChunksH - 1, fy + r);

                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        if (r != 0 && x != minX && x != maxX && y != minY && y != maxY)
                        {
                            continue;
                        }

                        int idx = WorldConstants.ChunkIndex(x, y);
                        if (_have.Get(idx))
                        {
                            continue;
                        }

                        if (!isChunkReady(idx))
                        {
                            continue;
                        }

                        int estimatedBytes = estimateChunkBytes(idx);
                        if (estimatedBytes > remainingBytes)
                        {
                            _counters?.IncrementSchedulerBudgetDrops();
                            continue;
                        }

                        remainingBytes -= estimatedBytes;
                        remainingMessages--;
                        return idx;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Small fixed bitset for 1024 chunks.
        /// </summary>
        private struct BitSet1024
        {
            private ulong _a0;
            private ulong _a1;
            private ulong _a2;
            private ulong _a3;
            private ulong _a4;
            private ulong _a5;
            private ulong _a6;
            private ulong _a7;
            private ulong _a8;
            private ulong _a9;
            private ulong _a10;
            private ulong _a11;
            private ulong _a12;
            private ulong _a13;
            private ulong _a14;
            private ulong _a15;

            public readonly bool Get(int idx)
            {
                int word = idx >> 6;
                int bit = idx & 63;
                ulong mask = 1ul << bit;
                return word switch
                {
                    0 => (_a0 & mask) != 0,
                    1 => (_a1 & mask) != 0,
                    2 => (_a2 & mask) != 0,
                    3 => (_a3 & mask) != 0,
                    4 => (_a4 & mask) != 0,
                    5 => (_a5 & mask) != 0,
                    6 => (_a6 & mask) != 0,
                    7 => (_a7 & mask) != 0,
                    8 => (_a8 & mask) != 0,
                    9 => (_a9 & mask) != 0,
                    10 => (_a10 & mask) != 0,
                    11 => (_a11 & mask) != 0,
                    12 => (_a12 & mask) != 0,
                    13 => (_a13 & mask) != 0,
                    14 => (_a14 & mask) != 0,
                    _ => (_a15 & mask) != 0,
                };
            }

            public void Set(int idx)
            {
                int word = idx >> 6;
                int bit = idx & 63;
                ulong mask = 1ul << bit;
                switch (word)
                {
                    case 0: _a0 |= mask; break;
                    case 1: _a1 |= mask; break;
                    case 2: _a2 |= mask; break;
                    case 3: _a3 |= mask; break;
                    case 4: _a4 |= mask; break;
                    case 5: _a5 |= mask; break;
                    case 6: _a6 |= mask; break;
                    case 7: _a7 |= mask; break;
                    case 8: _a8 |= mask; break;
                    case 9: _a9 |= mask; break;
                    case 10: _a10 |= mask; break;
                    case 11: _a11 |= mask; break;
                    case 12: _a12 |= mask; break;
                    case 13: _a13 |= mask; break;
                    case 14: _a14 |= mask; break;
                    default: _a15 |= mask; break;
                }
            }
        }
    }
}
