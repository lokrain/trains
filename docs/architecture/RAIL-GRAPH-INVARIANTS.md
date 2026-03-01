# Rail Graph Invariants and Diagnostics

## Core invariants

1. Every active dense segment id resolves in id-to-dense map.
2. Every dense segment endpoint node resolves to a valid node position.
3. Every dense segment maps to exactly one tile-edge spatial key entry.
4. Segment adjacency is present in both endpoint node lists.
5. Spatial index key resolves back to the same stable segment id.

## Mutation guard

- Enable with `TileCenterRailGraph.EnableMutationGuard(true)`.
- Wrap authoritative writes with:
  - `BeginMutation()`
  - mutation operations
  - `EndMutation()`

In dev flows, writes outside mutation phase throw `InvalidOperationException`.

## Validation API

Use `ValidateInvariants(out string diagnostic)` after stress runs.

- `true` => graph is internally consistent.
- `false` => inspect diagnostic message for first failing invariant.

## Common failure diagnostics

- `Missing node position for segment ...`
  - Node table drift or stale node id in segment store.
- `Spatial index mismatch for segment ...`
  - Tile-edge index not updated atomically with segment mutation.
- `Missing adjacency link for segment ...`
  - Adjacency unlink/link path corruption or partial rollback.
