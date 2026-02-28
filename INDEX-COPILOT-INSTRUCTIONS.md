# ?? Copilot Instructions - Complete Index

This document provides a complete index of all Copilot instruction files and documentation.

---

## ?? Start Here

### New to the Project?
1. **Read**: `GET-STARTED-WITH-COPILOT.md` (10-15 min)
2. **Skim**: `.github/copilot-instructions.md` (20-30 min)
3. **Try**: Generate your first component with Copilot (5 min)

### Quick Reference Needed?
- **Cheat Sheet**: `COPILOT-INSTRUCTIONS-README.md`
- **Patterns**: See domain-specific guides below

### Want to Verify Installation?
- **Run**: `.\validate-copilot-instructions.ps1`
- **Check**: `SUMMARY-COPILOT-INSTRUCTIONS.md`

---

## ?? Documentation Files

| File | Purpose | Size | When to Read |
|------|---------|------|--------------|
| `GET-STARTED-WITH-COPILOT.md` | Getting started guide | 11 KB | **First time** |
| `COPILOT-INSTRUCTIONS-README.md` | Quick reference | 8 KB | **Daily** |
| `.github/copilot-instructions.md` | Main project standards | 15 KB | **Week 1** |
| `COPILOT-INSTRUCTIONS-INSTALLATION.md` | Installation summary | 6 KB | After install |
| `SUMMARY-COPILOT-INSTRUCTIONS.md` | Overview & metrics | 8 KB | After install |
| `INDEX-COPILOT-INSTRUCTIONS.md` | This file | - | Anytime |

---

## ?? Domain-Specific Guides

| Domain | File | Size | Covers |
|--------|------|------|--------|
| **Components** | `Assets/Scripts/Components/.copilot-instructions.md` | 5 KB | Pure data structs, naming, buffer elements |
| **Systems** | `Assets/Scripts/Systems/.copilot-instructions.md` | 10 KB | Burst systems, jobs, queries, ECB |
| **Aspects** | `Assets/Scripts/Aspects/.copilot-instructions.md` | 12 KB | Aspect patterns, encapsulation, composition |
| **Utilities** | `Assets/Scripts/Utilities/.copilot-instructions.md` | 14 KB | Math helpers, Burst utils, builders |
| **Converters** | `Assets/Scripts/Converters/.copilot-instructions.md` | 17 KB | Authoring components, bakers |

---

## ??? Templates & Tools

| File | Purpose |
|------|---------|
| `.github/copilot-instructions-template.md` | Template for new bounded contexts |
| `validate-copilot-instructions.ps1` | Validation script |

---

## ?? Quick Navigation

### By Use Case

#### "I want to create a new component"
1. Navigate to `Assets/Scripts/Components/`
2. Review `Components/.copilot-instructions.md`
3. Use prompt: "Create a [Name] component for [purpose] following component guidelines"

#### "I want to create a new system"
1. Navigate to `Assets/Scripts/Systems/`
2. Review `Systems/.copilot-instructions.md`
3. Use prompt: "Create a Burst-compiled system to [action] in [UpdateGroup]"

#### "I want to create an aspect"
1. Navigate to `Assets/Scripts/Aspects/`
2. Review `Aspects/.copilot-instructions.md`
3. Use prompt: "Create a [Name]Aspect for [behavior]"

#### "I want to add utilities"
1. Navigate to `Assets/Scripts/Utilities/`
2. Review `Utilities/.copilot-instructions.md`
3. Use prompt: "Create [type] utilities for [purpose] with Burst support"

#### "I want to add authoring support"
1. Navigate to `Assets/Scripts/Converters/`
2. Review `Converters/.copilot-instructions.md`
3. Use prompt: "Create authoring and baker for [entity type] with [fields]"

### By Learning Path

#### Week 1: Fundamentals
- [ ] Day 1: Read `GET-STARTED-WITH-COPILOT.md`
- [ ] Day 2: Read `.github/copilot-instructions.md` (Non-negotiables section)
- [ ] Day 3: Read `Components/.copilot-instructions.md` + generate 5 components
- [ ] Day 4: Read `Systems/.copilot-instructions.md` + generate 3 systems
- [ ] Day 5: Review all generated code, refactor as needed

#### Week 2: Advanced Patterns
- [ ] Day 1: Read `Aspects/.copilot-instructions.md` + create 2 aspects
- [ ] Day 2: Read `Utilities/.copilot-instructions.md` + add utilities
- [ ] Day 3: Read `Converters/.copilot-instructions.md` + add authoring
- [ ] Day 4: Build a complete feature (component + system + aspect)
- [ ] Day 5: Profile and optimize, document learnings

#### Week 3: Mastery
- [ ] Build 2-3 complete features using Copilot
- [ ] Review and refactor all generated code
- [ ] Add custom patterns to instructions
- [ ] Share learnings with team

---

## ?? Statistics

### File Count: 12
- Documentation: 6 files
- Domain guides: 5 files
- Templates: 1 file
- Tools: 1 file (PowerShell script)

### Total Size: ~111 KB
- Documentation: ~38 KB
- Domain guides: ~63 KB
- Templates: ~6 KB
- Tools: ~4 KB

### Total Content: ~2,700 lines
- Code examples: 50+
- Anti-patterns: 25+
- Best practices: 100+

---

## ?? Learning Resources

### Internal Documentation
| Resource | Location |
|----------|----------|
| Main guidelines | `.github/copilot-instructions.md` |
| Quick reference | `COPILOT-INSTRUCTIONS-README.md` |
| Getting started | `GET-STARTED-WITH-COPILOT.md` |
| Component patterns | `Assets/Scripts/Components/.copilot-instructions.md` |
| System patterns | `Assets/Scripts/Systems/.copilot-instructions.md` |
| Aspect patterns | `Assets/Scripts/Aspects/.copilot-instructions.md` |
| Utility patterns | `Assets/Scripts/Utilities/.copilot-instructions.md` |
| Authoring patterns | `Assets/Scripts/Converters/.copilot-instructions.md` |

### External Resources
- [DOTS Best Practices](https://docs.unity3d.com/Packages/com.unity.entities@latest)
- [Burst Compiler](https://docs.unity3d.com/Packages/com.unity.burst@latest)
- [Entities Graphics](https://docs.unity3d.com/Packages/com.unity.entities.graphics@latest)
- [Unity Mathematics](https://docs.unity3d.com/Packages/com.unity.mathematics@latest)

---

## ? Cheat Sheets

### DO ?
- Use unmanaged types in components (`float3`, `int2`)
- Mark systems with `[BurstCompile]`
- Use `IJobEntity` for parallel processing
- Keep systems stateless
- Use `EntityCommandBuffer` for structural changes
- Use `IEnableableComponent` for toggleable states
- Document all public APIs with XML comments
- Use Unity.Mathematics (`math.*`)

### DON'T ?
- Use managed types in components (`string`, `List<T>`)
- Add/remove components in hot loops
- Store state in systems
- Use `UnityEngine.Mathf` (use `math.*` instead)
- Use `UnityEngine.Random` (use `Unity.Mathematics.Random`)
- Use LINQ in hot paths
- Nest queries
- Access EntityManager in jobs

---

## ?? Search Guide

### Need to Find...

#### "How to create X"
1. Check `COPILOT-INSTRUCTIONS-README.md` quick reference
2. Review domain-specific guide (Components, Systems, etc.)
3. Search for "Example:" or "Pattern:" in guide

#### "What's the naming convention for X"
1. Check `COPILOT-INSTRUCTIONS-README.md` ? "Naming Conventions" table
2. Or `.github/copilot-instructions.md` ? "Code Style" section

#### "Is X an anti-pattern?"
1. Search domain guide for "Anti-Patterns" section
2. Check for ? examples

#### "How to integrate with Y context"
1. Check `.github/copilot-instructions.md` ? "Architecture Rules"
2. Review bounded context dependencies

---

## ?? Example Prompts by Domain

### Components
```
Create a Velocity component for entity movement
Create a GridPosition component for tile-based positioning
Create an enableable IsSelected component for marking selected entities
Create a DynamicBuffer element for storing waypoints
```

### Systems
```
Create a Burst-compiled system to update positions based on velocity
Create a system to process spawner requests using EntityCommandBuffer
Create a parallel job to calculate pathfinding for entities
Create a system to handle collision detection in FixedStepSimulationSystemGroup
```

### Aspects
```
Create a VehicleAspect for managing vehicle movement
Create a BuildingAspect for construction validation
Create a PathfindingAspect with A* pathfinding logic
Create a TrainAspect for movement along tracks
```

### Utilities
```
Create grid utilities for world-to-grid coordinate conversion
Create math utilities for bezier curve interpolation
Create pathfinding utilities for A* heuristics with Burst
Create random utilities for spawning points in circles
```

### Converters
```
Create authoring and baker for vehicle spawning
Create authoring and baker for track segments with transforms
Create authoring and baker for building placement
Create authoring and baker for terrain tiles with height
```

---

## ?? Maintenance

### When to Update Instructions

#### Add New Patterns
When you discover a useful pattern:
1. Add to appropriate domain guide
2. Include code example
3. Document when to use
4. Commit to version control

#### Remove Anti-Patterns
When a mistake is made:
1. Add to "Anti-Patterns" section
2. Show incorrect example (?)
3. Show correct example (?)
4. Explain why

#### Adjust Standards
When project needs change:
1. Update `.github/copilot-instructions.md`
2. Update affected domain guides
3. Document reasoning in commit message
4. Notify team

---

## ?? Troubleshooting

### Copilot Not Following Instructions?
1. Check file location (domain-specific instructions load per folder)
2. Be more explicit in prompts ("following Burst guidelines")
3. Restart IDE (instructions reload on startup)
4. Verify instructions exist: Run `.\validate-copilot-instructions.ps1`

### Generated Code Has Issues?
1. Ask for refactoring: "Refactor to use Burst compilation"
2. Reference pattern: "Update to match IJobEntity pattern"
3. Request optimization: "Optimize for 10k entities"
4. Review relevant domain guide

### Can't Find a Pattern?
1. Search all guides: `Select-String -Path "Assets\Scripts\**\.copilot-instructions.md" -Pattern "your search"`
2. Check main guidelines: `.github/copilot-instructions.md`
3. Ask Copilot to generate based on existing patterns
4. Document new pattern for future

---

## ?? Support

### Quick Help
- **Validation**: Run `.\validate-copilot-instructions.ps1`
- **Quick Reference**: `COPILOT-INSTRUCTIONS-README.md`
- **Getting Started**: `GET-STARTED-WITH-COPILOT.md`

### Deep Dive
- **Architecture**: `.github/copilot-instructions.md`
- **Domain Patterns**: See domain-specific guides
- **Examples**: All guides include 5-10 examples each

---

## ?? Success Metrics

Track your progress:
- [ ] All 11 instruction files installed
- [ ] Main guidelines read (`.github/copilot-instructions.md`)
- [ ] First component generated with Copilot
- [ ] First system generated with Copilot
- [ ] Complete feature built (components + systems + aspects)
- [ ] Code profiled and optimized
- [ ] Custom patterns added to instructions
- [ ] Team trained on Copilot usage

---

**Installation Date**: 2025-01-01  
**Unity Version**: 6.5 (Alpha)  
**DOTS Version**: 1.x  
**Total Files**: 12  
**Total Size**: ~111 KB  
**Total Lines**: ~2,700  

**Happy Coding! ?????**
