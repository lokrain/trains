# ADR 0008: Entities Graphics Chunk Rendering Architecture

- Status: Accepted
- Date: 2026-03-01
- Supersedes: None
- Related: `docs/sprints/Sprint4-Closure-Evidence.md`

## Context

Client world rendering must scale with chunked world data streamed via snapshot/patch networking.

Per-tile entities are too expensive for expected world sizes and update rates. Rendering needs bounded rebuild behavior and deterministic chunk-level update semantics.

## Decision

1. Use chunk-level presenter entities for terrain rendering (one entity per visible chunk).
2. Do not create per-tile render entities.
3. Use Entities Graphics bindings (`RenderMeshArray`, `MaterialMeshInfo`) at presenter level.
4. Mesh generation supports:
   - full rebuild for bootstrap/snapshot-full invalidation
   - partial rect rebuild for patch invalidation
5. Use version fence semantics between world snapshot version and mesh commit path:
   - track pending/rendered versions
   - discard stale/lower-version invalidations
6. Use per-frame rebuild budget controls to prevent frame collapse under patch storms.

## Rationale

- Chunk-level entities reduce structural-change pressure and draw setup overhead.
- Rect rebuild aligns with patch rect semantics from transport layer.
- Version fencing prevents visual rollback from out-of-order work completion.
- Budgeting keeps frame-time bounded under burst update conditions.

## Consequences

### Positive

- Better runtime scalability than per-tile entity rendering.
- Direct alignment with network chunk ownership and dirty metadata.
- Clear surfaces for profiling (build queue, rebuild count/frame, commit latency).

### Negative

- Mesh patching logic is more complex than full rebuild only.
- Requires additional correctness tests (equivalence and seam continuity).

## Verification

- Mesh determinism hash tests.
- Partial-vs-full equivalence tests.
- Border seam continuity tests.
- Patch-storm budget tests with queue carryover.
