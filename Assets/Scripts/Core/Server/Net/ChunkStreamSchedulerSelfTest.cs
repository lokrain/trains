#nullable enable
using OpenTTD.Core.Net.Protocol;
using OpenTTD.Core.World;

namespace OpenTTD.Core.Server.Net
{
    /// <summary>
    /// Minimal self-test for scheduler budget behavior and join-ready fence semantics.
    /// </summary>
    public static class ChunkStreamSchedulerSelfTest
    {
        public static bool Run()
        {
            var counters = new ReplicationCounters();
            var scheduler = new ChunkStreamScheduler(ringRadius: 1, counters: counters);
            scheduler.SetFocusChunk(0, 0);

            int low = WorldConstants.ChunkIndex(0, 0);
            int high = WorldConstants.ChunkIndex(1, 0);

            scheduler.EnqueueResync(high);

            int remainingBytes = 4;
            int remainingMessages = 1;

            int selected = scheduler.NextChunkToSendBudgeted(
                static _ => true,
                idx => idx == high ? 8 : 2,
                ref remainingBytes,
                ref remainingMessages);

            if (selected != low)
            {
                return false;
            }

            if (remainingBytes != 2 || remainingMessages != 0)
            {
                return false;
            }

            if (counters.SchedulerBudgetDrops != 1)
            {
                return false;
            }

            if (scheduler.UpdateJoinReadyFence(requiredRadius: 0))
            {
                return false;
            }

            scheduler.MarkHave(low);
            if (!scheduler.UpdateJoinReadyFence(requiredRadius: 0))
            {
                return false;
            }

            scheduler.ResetJoinReadyFence();
            return !scheduler.IsJoinReady;
        }
    }
}
