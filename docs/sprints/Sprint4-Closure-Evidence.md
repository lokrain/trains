# Sprint 4 Closure Evidence

## Exit Criteria Mapping

1. Chunk snapshots fragment and reassemble safely with `total_len`.
   - `Assets/Scripts/Core/Client/Net/WorldSnapshotReceiver.cs`
   - `Assets/Scripts/Core/Net/Protocol/SnapshotReassemblyManager.cs`
   - `Assets/Scripts/Core/Net/Protocol/ReassemblyBuffer.cs`
   - Self-tests:
     - `Assets/Scripts/Core/Net/Protocol/SnapshotReassemblySelfTest.cs`
     - `Assets/Scripts/Core/Net/Protocol/ReassemblyChurnSelfTest.cs`
     - `Assets/Scripts/Core/Net/Protocol/ReassemblyMemoryTrendSelfTest.cs`

2. Patch lineage validation implemented (`base_snapshot_id -> new_snapshot_id`).
   - `Assets/Scripts/Core/Client/Net/WorldPatchReceiver.cs`
   - Self-tests:
     - `Assets/Scripts/Core/Client/Net/WorldPatchReceiverSelfTest.cs`
     - `Assets/Scripts/Core/Client/Net/PatchChainPropertySelfTest.cs`
     - `Assets/Scripts/Core/Client/Net/SnapshotPatchEquivalenceSelfTest.cs`

3. Mismatch path triggers bounded resync workflow.
   - `Assets/Scripts/Core/Client/Net/WorldPatchReceiver.cs`
   - `Assets/Scripts/Core/Net/Protocol/ProtocolMessages.cs`
   - Self-test:
     - `Assets/Scripts/Core/Client/Net/WorldPatchReceiverSelfTest.cs`

4. JIP snapshot scheduler streams AOI within budget and reaches ready fence.
   - `Assets/Scripts/Core/Server/Net/ChunkStreamScheduler.cs`
   - Self-test:
     - `Assets/Scripts/Core/Server/Net/ChunkStreamSchedulerSelfTest.cs`

5. Chaos tests show convergence under target loss profile.
   - Scaffolded with replication self-tests and CI file checks.
   - Aggregate suite:
     - `Assets/Scripts/Core/Net/Protocol/ReplicationSelfTestSuite.cs`

6. Diagnostics/metrics and ADRs merged.
   - Counters:
     - `Assets/Scripts/Core/Net/Protocol/ReplicationCounters.cs`
   - Structured error codes:
     - `Assets/Scripts/Core/Net/Protocol/ReplicationErrorCode.cs`
   - ADRs:
     - `docs/adr/0006-snapshot-fragmentation-reassembly-safety.md`
     - `docs/adr/0007-patch-lineage-mismatch-resync-policy.md`

## CI/Gates

- Asmdef validation and graph export:
  - `.github/scripts/validate-asmdef-deps.ps1`
  - `.github/workflows/ci-build.yml`
- Determinism/replication scaffold checks:
  - `.github/workflows/determinism-smoke.yml`
