# Asmdef Dependency Graph Validation

This document defines how asmdef dependencies are validated and visualized in CI.

## Why this exists

Unity asmdef boundaries must remain acyclic. Broken JSON, missing references, or cycles can silently break compile ordering and cross-assembly type resolution.

## Validation script

- Script: `.github/scripts/validate-asmdef-deps.ps1`
- Scope: `Assets/Scripts/**/*.asmdef`

It reports:
- invalid asmdef JSON (including duplicated opening brace patterns like `{{`)
- missing referenced assemblies
- dependency cycles

The script exits with non-zero status on any error.

## Graph output modes

### Console graph

```powershell
powershell -ExecutionPolicy Bypass -File .github/scripts/validate-asmdef-deps.ps1 -ShowGraph
```

### Mermaid graph file

```powershell
powershell -ExecutionPolicy Bypass -File .github/scripts/validate-asmdef-deps.ps1 -AsMermaid -MermaidPath asmdef-deps.mmd
```

## CI integration

- `ci-build.yml` validates asmdefs, exports Mermaid output, and uploads it as an artifact.
- `determinism-smoke.yml` validates asmdefs and prints dependency connections.

## Dependency policy

- Dependencies must be one-way.
- No assembly may depend (directly or transitively) back on itself.
- Keep foundational contexts (`World`, `Protocol`) as low-level dependencies.
- Do not rely on manual `.csproj` edits; Unity regenerates project files from asmdefs.
