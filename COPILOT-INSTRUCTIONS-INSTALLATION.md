# Copilot Instructions - Installation Summary

## ? Successfully Created Instruction Files

All Copilot instruction files have been created for your Unity ECS/DOTS city builder project.

### Files Created

#### 1. Root Project Instructions
- **`.github/copilot-instructions.md`** (Main instructions - 500+ lines)
  - Non-negotiables (performance, Burst, zero-GC)
  - Architecture rules (bounded contexts, DDD)
  - Code style and documentation standards
  - Feature delivery protocol
  - Common patterns and anti-patterns
  - Resources and references

#### 2. Domain-Specific Instructions (5 files)

| File Location | Lines | Purpose |
|---------------|-------|---------|
| `Assets/Scripts/Components/.copilot-instructions.md` | ~150 | Component design patterns, naming, documentation |
| `Assets/Scripts/Systems/.copilot-instructions.md` | ~350 | System architecture, jobs, queries, performance |
| `Assets/Scripts/Aspects/.copilot-instructions.md` | ~300 | Aspect patterns, encapsulation, usage examples |
| `Assets/Scripts/Utilities/.copilot-instructions.md` | ~350 | Utilities, helpers, Burst-compatible math, builders |
| `Assets/Scripts/Converters/.copilot-instructions.md` | ~400 | Authoring components, bakers, GameObject?ECS conversion |

#### 3. Quick Reference Guide
- **`COPILOT-INSTRUCTIONS-README.md`** (Reference guide - 250+ lines)
  - Quick pattern reference
  - Cheat sheets for DO/DON'T
  - Performance targets
  - Naming conventions table
  - Example prompts for Copilot

### Total Documentation
- **6 instruction files**
- **~2,300+ lines** of comprehensive guidance
- **50+ code examples**
- **25+ anti-patterns** documented

## How to Use

### For Developers
1. Read **`.github/copilot-instructions.md`** first for project-wide standards
2. Refer to domain-specific `.copilot-instructions.md` when working in that folder
3. Use **`COPILOT-INSTRUCTIONS-README.md`** as a quick reference

### For GitHub Copilot
GitHub Copilot will automatically:
- Load `.github/copilot-instructions.md` for all files
- Load folder-specific `.copilot-instructions.md` when working in those directories
- Apply rules and patterns when generating code suggestions

### Example Workflows

#### Creating a New Component
1. Navigate to `Assets/Scripts/Components/`
2. Copilot reads both `.github/copilot-instructions.md` and `Components/.copilot-instructions.md`
3. Ask: "Create a Velocity component for vehicle movement"
4. Copilot generates code following all patterns

#### Creating a New System
1. Navigate to `Assets/Scripts/Systems/`
2. Ask: "Create a Burst-compiled system to move vehicles along tracks"
3. Copilot generates system with proper attributes, jobs, and documentation

#### Creating an Aspect
1. Navigate to `Assets/Scripts/Aspects/`
2. Ask: "Create a TrainAspect for managing train movement"
3. Copilot generates aspect with proper readonly struct pattern

## Key Features

### ? Data-Oriented Design
- Components are pure data (unmanaged structs)
- Systems are stateless
- Blob assets for read-only data
- NativeContainers for large collections

### ? Performance First
- Burst compilation everywhere
- Zero GC allocations in hot paths
- Parallel jobs for large entity sets
- EntityCommandBuffer for structural changes

### ? Best Practices
- XML documentation on all public APIs
- Enableable components for toggleable states
- Proper update group ordering
- Dependency injection via singletons

### ? Anti-Patterns Documented
- No managed types in components
- No state in systems
- No LINQ in hot paths
- No nested queries

## Verification

Run this command to see all instruction files:
```powershell
Get-ChildItem -Path . -Recurse -Filter "*copilot-instructions.md" | Select-Object FullName
```

Expected output:
```
.github\copilot-instructions.md
Assets\Scripts\Aspects\.copilot-instructions.md
Assets\Scripts\Components\.copilot-instructions.md
Assets\Scripts\Converters\.copilot-instructions.md
Assets\Scripts\Systems\.copilot-instructions.md
Assets\Scripts\Utilities\.copilot-instructions.md
```

## Next Steps

1. **Read the instructions**: Start with `.github/copilot-instructions.md`
2. **Test with Copilot**: Try asking for component/system generation
3. **Customize**: Adjust rules based on your team's preferences
4. **Expand**: Add more domain-specific instructions as new bounded contexts are added

## Bounded Contexts (Future)

When you create these folders, add `.copilot-instructions.md` to each:
- `Assets/Scripts/World/` - Terrain, tiles, chunks
- `Assets/Scripts/Build/` - Placement tools, undo/redo
- `Assets/Scripts/Transport/` - Tracks, pathfinding, signals
- `Assets/Scripts/Vehicles/` - Trains, buses, schedules
- `Assets/Scripts/Economy/` - Companies, cargo, towns
- `Assets/Scripts/UI/` - UI Toolkit integration
- `Assets/Scripts/SaveLoad/` - Serialization, replay
- `Assets/Scripts/Debug/` - Visualization, dev tools

## Support

### Quick Reference
See **`COPILOT-INSTRUCTIONS-README.md`** for:
- Pattern cheat sheet
- DO/DON'T checklist
- Performance targets
- Naming conventions
- Common attributes

### Example Prompts
```
"Create a component for storing grid position following the component guidelines"
"Add a Burst-compiled system to update velocities with gravity"
"Create an aspect for vehicle movement along tracks"
"Add grid utilities for converting world to grid coordinates with Burst support"
"Create an authoring component and baker for spawning vehicles"
```

---

**Installation Date**: 2025-01-01
**Unity Version**: 6.5 (Alpha)
**DOTS Version**: 1.x
**Total Lines**: ~2,300+
**Files Created**: 7
