#nullable enable
using OpenTTD.Core.Client.Net;
using OpenTTD.Core.Server.Net;

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Aggregated replication self-test suite for smoke validation hooks.
    /// </summary>
    public static class ReplicationSelfTestSuite
    {
        public static bool Run()
        {
            return Crc64SelfTest.Run()
                && ReplicationProtocolSelfTest.Run()
                && SnapshotReassemblySelfTest.Run()
                && ReassemblyChurnSelfTest.Run()
                && ReassemblyMemoryTrendSelfTest.Run()
                && WorldPatchReceiverSelfTest.Run()
                && SnapshotPatchEquivalenceSelfTest.Run()
                && PatchChainPropertySelfTest.Run()
                && ChunkStreamSchedulerSelfTest.Run();
        }
    }
}
