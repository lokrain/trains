#nullable enable

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Minimal long-churn reassembly self-test to detect obvious orphan-growth behavior.
    /// </summary>
    public static class ReassemblyMemoryTrendSelfTest
    {
        public static bool Run()
        {
            using var manager = new SnapshotReassemblyManager(timeoutTicks: 3);

            const int rounds = 32;
            const int perRound = 32;
            int totalExpectedEvictions = 0;
            int keyBase = 10000;

            for (int round = 0; round < rounds; round++)
            {
                for (int i = 0; i < perRound; i++)
                {
                    ulong key = (ulong)(keyBase++);
                    if (!manager.TryGetOrCreate(key, totalLen: 12, fragCount: 4, nowTick: (ulong)round, out ReassemblyBuffer? buf) || buf == null)
                    {
                        return false;
                    }

                    if (!buf.TryAdd(0, new byte[] { 1, 2, 3 }, fragOffset: 0))
                    {
                        return false;
                    }
                }

                totalExpectedEvictions += perRound;
                int evicted = manager.EvictExpired((ulong)round + 3UL);
                if (evicted != perRound)
                {
                    return false;
                }
            }

            int trailingEvicted = manager.EvictExpired((ulong)rounds + 10UL);
            return trailingEvicted == 0 && totalExpectedEvictions == rounds * perRound;
        }
    }
}
