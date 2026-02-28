# ADR 0007: Patch Lineage Validation and Mismatch Resync Policy

- Status: Accepted
- Date: 2026-02-28
- Supersedes: None
- Related: `docs/architecture/BINARY-PROTOCOL-V1.md`

## Context

Chunk patch streams are valid only when applied to the expected base snapshot lineage.

Packet delay/reorder and client divergence can produce base mismatches. Without bounded mismatch handling, clients can emit resync storms or apply invalid lineage.

## Decision

1. Patch lineage contract is strict:
   - patch carries `base_snapshot_id` and `new_snapshot_id`
   - client applies only when local snapshot id equals `base_snapshot_id`
   - accepted patch advances local id to `new_snapshot_id`
2. Non-advancing lineage (`new_snapshot_id <= base_snapshot_id`) is rejected.
3. Invalid rect bounds or payload shape is rejected before mutation.
4. Mismatch path triggers resync request flow with reason code.
5. Resync request policy is bounded by:
   - per-chunk debounce cooldown
   - global request cap over a sliding window
6. Failure/mismatch paths emit structured error codes and telemetry counters.

## Rationale

- Strict lineage prevents skipped or replayed patch chains.
- Bounded resync policy prevents mismatch bursts from amplifying into traffic storms.
- Deterministic rejection codes improve observability and incident triage.

## Consequences

### Positive

- Consistent authoritative convergence behavior.
- Predictable and bounded mismatch recovery pressure.
- Better diagnostics for lineage and payload errors.

### Negative

- Temporary staleness can persist during cooldown windows.
- Additional counters and policy state increase implementation complexity.

## Verification

- Happy-path patch apply and lineage advance tests.
- Mismatch path debounced resync request tests.
- Burst mismatch cooldown/global-cap tests.
- Patch-chain property tests for no skipped lineage acceptance.
