#nullable enable
using System;

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Lightweight replication counters for snapshot/patch/resync diagnostics.
    /// </summary>
    public sealed class ReplicationCounters
    {
        public long SnapshotFragmentsReceived;
        public long SnapshotReassemblyFailures;
        public long SnapshotTransferTimeoutEvictions;
        public long PatchApplies;
        public long PatchMismatches;
        public long ResyncRequestsIssued;
        public long ResyncRequestsSuppressed;
        public long SchedulerBudgetDrops;

        public void IncrementSnapshotFragmentsReceived()
        {
            SnapshotFragmentsReceived++;
        }

        public void IncrementSnapshotReassemblyFailures()
        {
            SnapshotReassemblyFailures++;
        }

        public void AddSnapshotTransferTimeoutEvictions(int count)
        {
            if (count > 0)
            {
                SnapshotTransferTimeoutEvictions += count;
            }
        }

        public void IncrementPatchApplies()
        {
            PatchApplies++;
        }

        public void IncrementPatchMismatches()
        {
            PatchMismatches++;
        }

        public void IncrementResyncRequestsIssued()
        {
            ResyncRequestsIssued++;
        }

        public void IncrementResyncRequestsSuppressed()
        {
            ResyncRequestsSuppressed++;
        }

        public void IncrementSchedulerBudgetDrops()
        {
            SchedulerBudgetDrops++;
        }
    }
}
