# ADR 0009: Rail Graph Storage Architecture

- Status: Accepted
- Date: 2026-03-01
- Supersedes: None

## Context

Rail topology requires high-frequency authoritative mutations with stable external identifiers and cache-friendly iteration. Data corruption risk increases under churn if adjacency and spatial indices drift.

## Decision

1. Use stable typed IDs (`SegmentId`, `NodeId`, `EdgeId`) at API boundaries.
2. Use dense SoA segment storage for hot iteration.
3. Use id-to-dense indirection for stable IDs with swap-remove compaction.
4. Use pooled adjacency records for node edge lists.
5. Use tile-edge O(1) spatial index for overlap checks and lookup.
6. Enforce optional explicit mutation phase guard in authoritative mutation path.
7. Provide invariant validator for segment-node-adjacency-spatial consistency.

## Rationale

- Stable IDs decouple network/event contracts from internal compaction.
- Dense SoA improves cache locality and throughput for scanning/updating segments.
- O(1) spatial index supports deterministic placement validation at scale.
- Invariant validation provides early corruption detection under stress tests.

## Consequences

### Positive

- Better throughput and data locality for large rail networks.
- Deterministic external identity with compact mutable internals.
- Clear integrity checks for QA and fuzz/property testing.

### Negative

- More complex bookkeeping than naive object graph model.
- Requires strict update ordering and index synchronization discipline.
