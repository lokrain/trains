#nullable enable
using System;
using System.Collections.Generic;

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Manages active snapshot fragment transfers with timeout and eviction.
    /// </summary>
    public sealed class SnapshotReassemblyManager : IDisposable
    {
        private sealed class Entry
        {
            public ReassemblyBuffer Buffer = null!;
            public ulong LastTouchedTick;
        }

        private readonly Dictionary<ulong, Entry> _entries = new(256);
        private readonly List<ulong> _evictKeys = new(32);
        private readonly ulong _timeoutTicks;

        public SnapshotReassemblyManager(ulong timeoutTicks)
        {
            _timeoutTicks = timeoutTicks == 0 ? 300UL : timeoutTicks;
        }

        public bool TryGetOrCreate(ulong transferKey, int totalLen, int fragCount, ulong nowTick, out ReassemblyBuffer? buffer)
        {
            if (_entries.TryGetValue(transferKey, out Entry entry))
            {
                entry.LastTouchedTick = nowTick;
                buffer = entry.Buffer;
                return true;
            }

            if (totalLen <= 0 || fragCount <= 0)
            {
                buffer = null;
                return false;
            }

            entry = new Entry
            {
                Buffer = new ReassemblyBuffer(totalLen, fragCount),
                LastTouchedTick = nowTick
            };
            _entries.Add(transferKey, entry);
            buffer = entry.Buffer;
            return true;
        }

        public bool TryGet(ulong transferKey, out ReassemblyBuffer? buffer)
        {
            if (_entries.TryGetValue(transferKey, out Entry entry))
            {
                buffer = entry.Buffer;
                return true;
            }

            buffer = null;
            return false;
        }

        public void Touch(ulong transferKey, ulong nowTick)
        {
            if (_entries.TryGetValue(transferKey, out Entry entry))
            {
                entry.LastTouchedTick = nowTick;
            }
        }

        public void Remove(ulong transferKey)
        {
            if (_entries.TryGetValue(transferKey, out Entry entry))
            {
                entry.Buffer.Dispose();
                _entries.Remove(transferKey);
            }
        }

        public int EvictExpired(ulong nowTick)
        {
            _evictKeys.Clear();
            foreach (KeyValuePair<ulong, Entry> kv in _entries)
            {
                if (nowTick - kv.Value.LastTouchedTick >= _timeoutTicks)
                {
                    _evictKeys.Add(kv.Key);
                }
            }

            for (int i = 0; i < _evictKeys.Count; i++)
            {
                Remove(_evictKeys[i]);
            }

            return _evictKeys.Count;
        }

        public void Dispose()
        {
            foreach (KeyValuePair<ulong, Entry> kv in _entries)
            {
                kv.Value.Buffer.Dispose();
            }

            _entries.Clear();
        }
    }
}
