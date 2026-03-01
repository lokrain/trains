# Rendering Runbook and Tuning Guide

## Scope

Operational guide for chunk mesh rendering in client presentation path.

## Runtime knobs

### Initial bootstrap budget
- Source: `MapRenderConfigComponent.MaxChunkBuildsPerFrame`
- Effect: limits initial full chunk mesh builds per frame.
- Recommendation: start at `32`, tune down for low-end targets.

### Invalidation budget
- Source: `ChunkMeshRebuildBudget.MaxInvalidationsPerFrame`
- Effect: limits dirty invalidation processing per frame with queue carryover.
- Recommendation: start at `256`, tune based on patch storm profile.

## Debug checks

- Validate chunk dirty flow:
  - `ChunkDirtyFlags.Render` set on patch/snapshot apply
  - dirty rect bounds expected after patch apply
- Validate no stale invalidation regressions:
  - pending version should never decrease
- Validate seam continuity:
  - run `ChunkMeshSeamSelfTest`

## Troubleshooting

### Symptom: frame spikes during heavy patch bursts
- Lower `MaxInvalidationsPerFrame`
- Lower `MaxChunkBuildsPerFrame`
- Verify no accidental full rebuild path for rect invalidations

### Symptom: visual rollback/flicker
- Verify version fence logic in invalidation processing
- Ensure stale invalidations are ignored

### Symptom: chunk border cracks
- Run seam/equivalence self-tests
- Verify dirty rect expands by +1 vertex halo in partial updates

## Validation checklist

- Build succeeds.
- Asmdef dependency validation passes.
- Determinism-smoke scaffold file checks include rendering self-tests.
- Render benchmark EditMode tests (`RenderBench`) can be executed manually via Unity CLI.
