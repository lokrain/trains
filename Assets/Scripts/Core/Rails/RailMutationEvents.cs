using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Rail mutation operation type for event hooks.
    /// </summary>
    public enum RailMutationOp : byte
    {
        Create = 0,
        Update = 1,
        Remove = 2
    }

    /// <summary>
    /// Deterministic rail mutation payload used by event-pipeline stubs.
    /// </summary>
    public struct RailMutationEvent
    {
        public RailMutationOp Op;
        public SegmentId SegmentId;
        public SegmentSpec16 SegmentSpec;
        public uint TopologyVersion;
    }

    /// <summary>
    /// Utility helpers for rail mutation event queues.
    /// </summary>
    public static class RailMutationEventBuffer
    {
        public static void Drain(ref NativeList<RailMutationEvent> source, ref NativeList<RailMutationEvent> destination)
        {
            for (int i = 0; i < source.Length; i++)
            {
                destination.Add(source[i]);
            }

            source.Clear();
        }
    }
}
