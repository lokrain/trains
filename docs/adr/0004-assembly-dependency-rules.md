# ADR 0004: Assembly Dependency Rules

## Status
Accepted

## Decision
Create bounded-context assemblies with explicit one-way references and no cycles.

## Consequences
- Stronger module boundaries for long-term maintainability.
- Clear context ownership and dependency policy.
- Reduced accidental coupling across simulation, net, and presentation domains.
