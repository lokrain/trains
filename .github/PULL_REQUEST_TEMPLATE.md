## Summary
- 

## Validation
- [ ] Workspace build passes
- [ ] Determinism impact assessed
- [ ] Perf impact assessed

## Determinism / Simulation
- [ ] No wall-clock logic in authoritative path
- [ ] Stable ordering preserved for affected logic
- [ ] RNG stream/tag usage explicit and deterministic

## Performance
- [ ] No hot-path managed allocations introduced
- [ ] Structural changes batched/deferred where needed

## Networking / Protocol (if applicable)
- [ ] LE wire encoding preserved
- [ ] Bounds checks added for new decode paths
- [ ] Version/compatibility handling considered

## Migration Safety
- [ ] No owning `NativeArray` added to `IComponentData`
- [ ] No per-tile entity spawning introduced in new rendering path
