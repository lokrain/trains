# ADR 0002: RNG Substream Partitioning

## Status
Accepted

## Decision
Derive deterministic RNG substreams from base seed + stream tag.
Use explicit stream objects with internal counters; avoid global mutable RNG state.

## Consequences
- Reproducible random sequences per subsystem.
- Deterministic replay stability for same seed + input stream.
- Burst-safe random generation in hot systems.
