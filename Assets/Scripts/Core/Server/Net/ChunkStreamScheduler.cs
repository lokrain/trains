#nullable enable
using System;
using System.Collections.Generic;
using Unity.Mathematics;
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

        /// <summary>
        /// Creates a new chunk stream scheduler.
        /// </summary>
        /// <param name="ringRadius">Priority radius around current focus chunk.</param>
        public ChunkStreamScheduler(int ringRadius = 3)
        {
            _have = new BitSet1024();
            _high = new Queue<int>(64);
            _focus = new int2(0, 0);
            _ringRadius = ringRadius;
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
        /// Gets next chunk index to send snapshot for.
        /// Returns -1 when none are eligible.
        /// </summary>
        /// <param name="isChunkReady">Chunk readiness predicate.</param>
        public int NextChunkToSend(Func<int, bool> isChunkReady)
        {
            while (_high.Count > 0)
            {
                int idx = _high.Dequeue();
                if (isChunkReady(idx))
                {
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

            public bool Get(int idx)
            {
                int word = idx >> 6;
                int bit = idx & 63;
                ulong mask = 1ul << bit;
                switch (word)
                {
                    case 0: return (_a0 & mask) != 0;
                    case 1: return (_a1 & mask) != 0;
                    case 2: return (_a2 & mask) != 0;
                    case 3: return (_a3 & mask) != 0;
                    case 4: return (_a4 & mask) != 0;
                    case 5: return (_a5 & mask) != 0;
                    case 6: return (_a6 & mask) != 0;
                    case 7: return (_a7 & mask) != 0;
                    case 8: return (_a8 & mask) != 0;
                    case 9: return (_a9 & mask) != 0;
                    case 10: return (_a10 & mask) != 0;
                    case 11: return (_a11 & mask) != 0;
                    case 12: return (_a12 & mask) != 0;
                    case 13: return (_a13 & mask) != 0;
                    case 14: return (_a14 & mask) != 0;
                    default: return (_a15 & mask) != 0;
                }
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
