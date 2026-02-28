# ADR 0005: Asmdef Dependency Validation and Graphing

- Status: Accepted
- Date: 2026-02-28
- Supersedes: None
- Related: `docs/architecture/ASMDEF-DEPENDENCY-GRAPH.md`

## Context

Unity assembly definitions (`.asmdef`) define compilation boundaries and dependency order.

Recent issues showed that malformed asmdef JSON and dependency drift can cause:
- missing type resolution across assemblies
- accidental cycles in compile graph
- CI/build instability

Manual project-file edits (`.csproj`) are not durable in Unity because generated project files are regenerated from asmdefs.

## Decision

1. Validate asmdefs in CI using `.github/scripts/validate-asmdef-deps.ps1`.
2. Fail CI for:
   - invalid asmdef JSON
   - missing referenced assemblies
   - any dependency cycle
3. Export dependency graph as Mermaid artifact in CI.
4. Treat asmdefs as source of truth; do not rely on manual `.csproj` coupling.

## Rationale

- Keeps assembly graph deterministic and auditable.
- Prevents regressions from accidental malformed JSON (for example duplicate opening braces like `{{`).
- Makes dependency topology visible for architecture reviews.

## Consequences

### Positive

- Faster diagnosis of assembly boundary issues.
- Repeatable and enforceable dependency policy.
- Clear graph output for refactoring planning.

### Negative

- Slight CI time increase.
- Requires developers to fix asmdef graph issues before merge.

## Implementation Notes

- CI workflows run:
  - `.github/scripts/validate-asmdef-deps.ps1 -ShowGraph`
  - `.github/scripts/validate-asmdef-deps.ps1 -AsMermaid -MermaidPath asmdef-deps.mmd`
- Mermaid graph is uploaded as CI artifact.

## Follow-up

- Keep asmdef dependencies one-way.
- Continue context split toward explicit bounded assemblies without circular references.
