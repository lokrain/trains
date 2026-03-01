# Architecture Reference

This directory contains production-reference architecture documents for the OpenTTD-like ECS/DOTS project.

## Documents

- `FULL-SCALE-ARCHITECTURE.md`
  - Full production architecture (client + server)
  - Locked decisions, chunked SoA map model, terraforming, rails, replication, and scope staging

- `DEDICATED-SERVER-ARCHITECTURE.md`
  - Server-only headless architecture
  - Fixed-tick authority, command validation pipeline, replication responsibilities, and operational readiness

- `NETWORKING-CHUNK-STREAMING-SCHEDULER.md`
  - Transport-level chunk streaming scheduler for up to 16 concurrent players
  - Join-in-progress flow, per-connection budgets, RO/US lane policy, fragmentation, and resync behavior

- `BINARY-PROTOCOL-V1.md`
  - Byte-level protocol envelope and message layouts for handshake, snapshots, patches, rail events, and train states
  - Fragmentation, reassembly constraints, and v1 validation/security rules

- `RAILS-TOPOLOGY-PRODUCTION-DAY0.md`
  - Deterministic mutable rail topology with stable IDs, compact SoA storage, and compaction-safe replication
  - Day-0 production checklist for allocator, adjacency pool, spatial edge index, and versioning

- `ASMDEF-DEPENDENCY-GRAPH.md`
  - Asmdef dependency validation, cycle detection, and Mermaid graph generation
  - CI enforcement policy and one-way dependency guidance

- `RENDERING-RUNBOOK.md`
  - Chunk rendering runtime tuning knobs and troubleshooting guidance
  - Bootstrap/invalidation budget controls and validation checklist

## Usage

Use these documents as the source of truth for:
- vertical-slice implementation ordering
- deterministic networking decisions
- ECS system group boundaries and update order
- map/chunk storage and derived-field recompute policy

## ADRs

- `../adr/README.md`
  - Sprint 1 architecture decisions for deterministic tick policy, RNG partitioning, protocol envelope v1, and asmdef dependency rules

## Current implementation policy

Phased delivery:
1. Implement dedicated-server bootstrap path in existing `Assembly-CSharp` for velocity.
2. After slice stabilization, split into server/client asmdefs with explicit boundaries.
