#nullable enable

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Minimal churn self-test for partial transfer eviction behavior.
    /// </summary>
    public static class ReassemblyChurnSelfTest
    {
        public static bool Run()
        {
            using var manager = new SnapshotReassemblyManager(timeoutTicks: 2);

            const int transferCount = 64;
            for (int i = 0; i < transferCount; i++)
            {
                ulong key = (ulong)(1000 + i);
                if (!manager.TryGetOrCreate(key, totalLen: 9, fragCount: 3, nowTick: 0, out ReassemblyBuffer? buf) || buf == null)
                {
                    return false;
                }

                if (!buf.TryAdd(0, new byte[] { 1, 2, 3 }, fragOffset: 0))
                {
                    return false;
                }
            }

            int firstEvicted = manager.EvictExpired(nowTick: 1);
            if (firstEvicted != 0)
            {
                return false;
            }

            int secondEvicted = manager.EvictExpired(nowTick: 2);
            return secondEvicted == transferCount;
        }
    }
}
