using System;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Orchestrates rail graph storage with stable IDs, node table, adjacency pool,
    /// dense segment storage, and tile-edge spatial index.
    /// </summary>
    public partial struct TileCenterRailGraph : IDisposable
    {
        /// <summary>
        /// Increments on logical topology mutation (add/remove).
        /// </summary>
        public uint TopologyVersion;

        /// <summary>
        /// Increments on internal compaction passes.
        /// </summary>
        public uint TopologyEpoch;

        private SegmentIdAllocator _segIds;
        private IdAllocator _nodeIds;

        private NodeTable _nodes;
        private AdjacencyPool _adj;
        private SegmentStoreSoA _segs;
        private RailSpatialIndex _spatial;
        private NativeList<RailMutationEvent> _mutationEvents;
        private byte _mutationPhase;
        private byte _enforceMutationPhase;

        /// <summary>
        /// Creates a tile-center rail graph.
        /// </summary>
        public static TileCenterRailGraph Create(Allocator alloc, int segCap = 4096, int nodeCap = 4096, int adjCap = 16384)
        {
            return new TileCenterRailGraph
            {
                TopologyVersion = 0,
                TopologyEpoch = 0,
                _segIds = SegmentIdAllocator.Create(1, alloc, segCap * 2),
                _nodeIds = IdAllocator.Create(1, alloc),
                _nodes = NodeTable.Create(nodeCap, alloc),
                _adj = AdjacencyPool.Create(adjCap, alloc),
                _segs = SegmentStoreSoA.Create(segCap, alloc),
                _spatial = RailSpatialIndex.Create(segCap * 4, alloc),
                _mutationEvents = new NativeList<RailMutationEvent>(segCap, alloc),
                _mutationPhase = 0,
                _enforceMutationPhase = 0
            };
        }

        /// <summary>
        /// Number of active segments.
        /// </summary>
        public readonly int SegmentCount => _segs.Count;

        /// <summary>
        /// Enables or disables mutation phase enforcement.
        /// </summary>
        public void EnableMutationGuard(bool enabled = true)
        {
            _enforceMutationPhase = (byte)(enabled ? 1 : 0);
        }

        /// <summary>
        /// Begins an explicit mutation phase.
        /// </summary>
        public void BeginMutation()
        {
            _mutationPhase = 1;
        }

        /// <summary>
        /// Ends an explicit mutation phase.
        /// </summary>
        public void EndMutation()
        {
            _mutationPhase = 0;
        }

        /// <summary>
        /// Drains buffered mutation events into destination list.
        /// </summary>
        public void DrainMutationEvents(ref NativeList<RailMutationEvent> destination)
        {
            RailMutationEventBuffer.Drain(ref _mutationEvents, ref destination);
        }

        public void Dispose()
        {
            _segIds.Dispose();
            _nodeIds.Dispose();
            _nodes.Dispose();
            _adj.Dispose();
            _segs.Dispose();
            _spatial.Dispose();
            if (_mutationEvents.IsCreated)
            {
                _mutationEvents.Dispose();
            }
        }
    }
}
