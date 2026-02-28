# ADR 0006: Snapshot Fragmentation and Reassembly Safety

- Status: Accepted
- Date: 2026-02-28
- Supersedes: None
- Related: `docs/architecture/NETWORKING-CHUNK-STREAMING-SCHEDULER.md`, `docs/architecture/BINARY-PROTOCOL-V1.md`

## Context

World snapshots are transported as chunk fragment streams over network lanes that may reorder, duplicate, or drop packets.

To remain production-safe under packet instability, the client reassembly path must be bounded, reject malformed metadata, and fail deterministically.

## Decision

1. `total_len` is mandatory in snapshot fragment streams.
2. Reassembly state is keyed per transfer identity and uses bounded pooled buffers.
3. Client validates fragment metadata before buffer writes:
   - valid codec
   - positive and bounded lengths
   - fragment payload length consistency
   - computed offsets/end bounds within `total_len`
4. Reassembly transfer timeout eviction is required.
5. Snapshot decode/apply failures return deterministic error codes and increment replication counters.
6. Completed transfer buffers are always cleaned up.

## Rationale

- `total_len` provides deterministic final buffer sizing and bounds checks.
- Early metadata validation prevents malformed packets from causing crashes or out-of-range writes.
- Timeout eviction prevents orphan transfer memory growth.

## CRC Placement

- Snapshot payload integrity verification occurs at codec decode stage before world apply.
- Corrupt or undecodable payloads are rejected before mutating world state.

## Consequences

### Positive

- Safe reassembly under reorder/dup/loss.
- Deterministic failure behavior and diagnostics.
- Reduced risk of memory growth from partial transfers.

### Negative

- Additional validation checks add small CPU overhead.
- Transfer eviction policy requires tuning against real latency/loss profiles.

## Verification

- Fragment metadata contract validation tests.
- Reorder/dup/partial transfer completion tests.
- Timeout eviction and leak-churn tests.
- Decode/apply failure path diagnostics verification.
