# Contributing

## Determinism checklist
- Do not use wall-clock decisions in authoritative simulation.
- Keep deterministic ordering (stable iteration by id/index where required).
- Use deterministic RNG streams (`DeterministicRngStreams`) with explicit tags.
- Add or update replay/hash checks for simulation-impacting changes.

## Performance checklist
- No per-frame managed allocations in hot paths.
- Prefer Burst-compatible code in jobs/systems.
- Avoid structural changes in tight loops; batch via ECB.

## Networking checklist
- Add strict bounds checks for all payload parsers.
- Keep little-endian canonical encoding.
- Preserve backward-safe version checks for envelope/payload.

## Pull requests
- Include build result.
- Include deterministic/perf impact note.
- Include tests or validation notes.
- Ensure Unity EditMode/PlayMode CI tests are green for internal branches.
- Fork PRs may skip Unity tests when repository secrets are unavailable.
