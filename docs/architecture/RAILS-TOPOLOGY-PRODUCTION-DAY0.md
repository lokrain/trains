# Rails Topology — Production from Day 0 (Mutable, Deterministic, Compactable)

## 1) Non-negotiable properties
1. Deterministic `SegmentId` across join-in-progress, replay, and resync.
2. Fast add/remove for frequent player edits.
3. Fast pathfinding expansion for future AI and congestion logic.
4. Fast tile-edge validation for build tooling and server rules.
5. Compact memory behavior over long sessions.
6. Network-correct topology reconstruction on clients.

---

## 2) Deterministic IDs

### 2.1 SegmentId is not array index
- Dense array index is mutable under compaction/swap-remove.
- `SegmentId` is a stable `u32` from authoritative server allocator.

### 2.2 SegmentId allocation
Server allocator state:
- `next_segment_id: u32`
- `free_segment_ids`: deterministic FIFO free list

Allocation order:
1. if free list non-empty: pop front
2. else: return `next_segment_id++`

Determinism rule:
- only server allocates IDs; clients consume replicated IDs.

---

## 3) Storage: SoA + indirection

### 3.1 Dense SoA arrays
- `dense_a_node[]`
- `dense_b_node[]`
- `dense_kind[]`
- `dense_flags[]`
- `dense_speed_class[]`
- `dense_id[]` (`SegmentId` per dense slot)

### 3.2 Indirection maps
- `id_to_dense: HashMap<SegmentId, DenseIndex>`
- `dense_to_id`: same data as `dense_id[]`

Removal behavior:
- swap-remove dense slot
- update `id_to_dense` for swapped segment

Outcome:
- stable external IDs
- compact SIMD-friendly dense arrays

---

## 4) Node adjacency

### 4.1 NodeId
- Stable `u32` from deterministic allocator (`next_node_id + free list`).
- Nodes created on endpoint demand.

### 4.2 Adjacency structure
Per-node linked edge records from pool:
- `node_head[NodeId] -> EdgeRecIndex`

EdgeRec fields:
- `next: EdgeRecIndex`
- `seg_id: SegmentId`
- `dir_flags: u8` (optional directed arc flags)

Removal:
- unlink matching `seg_id` from endpoint node lists (`O(degree)`).

Pool policy:
- persistent pool + free list
- no runtime heap allocations in tick path

---

## 5) Tile-edge spatial index (authoritative)

### 5.1 `TileEdgeKey` (`u32`)
Bit packing recommendation:
- `x` (11 bits)
- `y` (11 bits)
- `dir` (1 bit, H/V)
- `layer` (2 bits reserved)

### 5.2 `edge_to_seg`
- `NativeParallelHashMap<u32, SegmentId> edge_to_seg`

Usage:
- O(1) collision/occupancy check
- O(1) cursor hit test to segment

---

## 6) Compaction model

Compaction targets:
- adjacency edge pool
- node pool (when sparse)
- free-list housekeeping

### 6.1 Triggering
Deterministic trigger only:
- every `N` ticks, or
- fragmentation ratio threshold

### 6.2 TopologyEpoch
- `RailTopologyEpoch: u32` increments on compaction events.
- `SegmentId` remains stable across epoch bumps.
- Dense indices remain private and unreplicated.

Compaction guarantee:
- internal storage changes only; logical graph unchanged.

---

## 7) Replication contract

### 7.1 Required events
- `RailSegmentAdded { segment_id, endpoints, kind, flags, speed_class }`
- `RailSegmentRemoved { segment_id }`
- `RailTopologyEpochBumped { epoch }` (optional but recommended)

JIP baseline:
- full segment stream including stable IDs.

### 7.2 Wire encoding alignment
Protocol `SegmentSpec16` includes `segment_id` as final `u32` field.

---

## 8) Pathfinding compatibility
Path expansion reads by IDs:
- `NodeId -> EdgeRec -> seg_id -> other_node`

Cost lookup:
- `denseIndex = id_to_dense(seg_id)`
- read cost fields from dense SoA

Cache invalidation:
- `RailTopologyVersion` increments on add/remove
- `RailTopologyEpoch` increments on compaction (optional invalidation fence)

---

## 9) Deterministic safety rules
1. Only server allocates node/segment IDs.
2. Clients never synthesize IDs.
3. Topology changes are event-sourced and replayable.
4. Dense indices are private implementation detail.

---

## 10) Day-0 implementation checklist
- [ ] SegmentId allocator with deterministic FIFO free list
- [ ] Dense SoA segment storage + `id_to_dense`
- [ ] Node table + deterministic NodeId allocator
- [ ] EdgeRec pool + free list
- [ ] `edge_to_seg` authoritative index
- [ ] Add/remove update all views atomically
- [ ] JIP baseline + incremental topology events
- [ ] `RailTopologyVersion` + optional `RailTopologyEpoch`
- [ ] Zero allocations per tick on server hot paths
