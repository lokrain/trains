using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Encoded rail mutation delta record for replication stream generation.
    /// </summary>
    public struct RailSegmentDelta
    {
        public RailMutationOp Op;
        public SegmentId SegmentId;
        public SegmentSpec16 SegmentSpec;
        public uint TopologyVersion;
    }

    /// <summary>
    /// Encodes drained rail mutation events into compact delta records.
    /// </summary>
    public static class RailSegmentDeltaEncoder
    {
        public static void Encode(ref NativeList<RailMutationEvent> events, ref NativeList<RailSegmentDelta> output)
        {
            for (int i = 0; i < events.Length; i++)
            {
                RailMutationEvent ev = events[i];
                output.Add(new RailSegmentDelta
                {
                    Op = ev.Op,
                    SegmentId = ev.SegmentId,
                    SegmentSpec = ev.SegmentSpec,
                    TopologyVersion = ev.TopologyVersion
                });
            }
        }
    }
}
