# ADR 0001: Deterministic Tick Policy

## Status
Accepted

## Decision
Use fixed-step deterministic simulation groups with explicit ordering:
`SimInitGroup -> SimTickGroup -> PostSimGroup -> NetSendGroup`.

## Consequences
- Authoritative simulation runs in fixed cadence.
- Tick ordering remains stable across environments.
- Systems relying on wall-clock behavior are disallowed in authoritative paths.
