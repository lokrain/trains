# Getting Started with Copilot Instructions

## ?? Installation Complete!

Your Unity ECS/DOTS city builder project now has comprehensive Copilot instructions installed.

### ?? What Was Installed

| Category | Files | Total Size | Lines of Code |
|----------|-------|------------|---------------|
| **Root Instructions** | 2 files | ~21 KB | ~550 lines |
| **Domain Instructions** | 5 files | ~60 KB | ~1,500 lines |
| **Documentation** | 2 files | ~14 KB | ~350 lines |
| **Templates** | 1 file | ~6 KB | ~150 lines |
| **TOTAL** | **10 files** | **~101 KB** | **~2,550 lines** |

---

## ?? File Overview

### 1. Start Here
- **`.github/copilot-instructions.md`** - Main project-wide standards (READ THIS FIRST!)
  - Non-negotiables (Burst, zero-GC, data-oriented)
  - Architecture rules (bounded contexts, DDD)
  - Code style, naming conventions
  - Feature delivery protocol

### 2. Quick Reference
- **`COPILOT-INSTRUCTIONS-README.md`** - Cheat sheets and quick patterns
  - DO/DON'T checklist
  - Common patterns
  - Performance targets
  - Example prompts for Copilot

### 3. Domain-Specific Guides
- **`Assets/Scripts/Components/.copilot-instructions.md`** - Component design
- **`Assets/Scripts/Systems/.copilot-instructions.md`** - System architecture
- **`Assets/Scripts/Aspects/.copilot-instructions.md`** - Aspect patterns
- **`Assets/Scripts/Utilities/.copilot-instructions.md`** - Utility helpers
- **`Assets/Scripts/Converters/.copilot-instructions.md`** - Authoring & baking

### 4. Installation & Templates
- **`COPILOT-INSTRUCTIONS-INSTALLATION.md`** - Installation summary (this context)
- **`.github/copilot-instructions-template.md`** - Template for new contexts

---

## ?? Quick Start (5 Minutes)

### Step 1: Verify Installation
```powershell
# Run this command to see all instruction files
Get-ChildItem -Path . -Recurse -Filter "*copilot*.md" | Select-Object FullName
```

**Expected output**: 10 files

### Step 2: Read Core Instructions (10 min)
1. Open **`.github/copilot-instructions.md`**
2. Skim the "Non-Negotiables" section
3. Review the "Common Patterns" section
4. Bookmark for reference

### Step 3: Try Your First Copilot Prompt (2 min)
Open a new file in `Assets/Scripts/Components/` and ask:

```
Create a Velocity component for entity movement following the component guidelines
```

**Expected output**:
```csharp
/// <summary>
/// Represents the velocity of an entity in meters per second.
/// </summary>
public struct Velocity : IComponentData {
    public float3 Value;
}
```

### Step 4: Test Domain-Specific Instructions (3 min)
Open a new file in `Assets/Scripts/Systems/` and ask:

```
Create a Burst-compiled system to update entity positions based on velocity
```

**Expected output**:
- System with `[BurstCompile]` attribute
- Proper update group
- Parallel query pattern
- XML documentation

---

## ?? Example Workflows

### Workflow 1: Creating a New Feature

**Scenario**: Add train movement along tracks

1. **Create Components** (`Components/`)
   ```
   Prompt: "Create components for train position on track"
   ```
   
2. **Create Systems** (`Systems/`)
   ```
   Prompt: "Create a Burst system to move trains along tracks"
   ```
   
3. **Create Aspect** (`Aspects/`)
   ```
   Prompt: "Create a TrainAspect for train movement behavior"
   ```
   
4. **Create Utilities** (`Utilities/`)
   ```
   Prompt: "Create track utilities for position interpolation with Burst"
   ```

### Workflow 2: Adding GameObject Authoring

**Scenario**: Create authoring for spawning vehicles

1. **Navigate to** `Converters/`
   
2. **Ask Copilot**:
   ```
   Create an authoring component and baker for vehicle spawning with configurable speed and mesh
   ```
   
3. **Copilot generates**:
   - `VehicleAuthoring` MonoBehaviour
   - `VehicleBaker` with proper TransformUsageFlags
   - Validation and tooltips

### Workflow 3: Optimizing Performance

**Scenario**: Convert a system to use parallel jobs

1. **Open existing system**
   
2. **Ask Copilot**:
   ```
   Refactor this system to use IJobEntity for parallel execution
   ```
   
3. **Copilot suggests**:
   - Extract to `IJobEntity` struct
   - Add `[BurstCompile]` attribute
   - Schedule with `ScheduleParallel`

---

## ? Validation Checklist

After installation, verify Copilot is using the instructions:

- [ ] **Components**: Generated components are unmanaged structs
- [ ] **Systems**: Generated systems have `[BurstCompile]` attribute
- [ ] **Documentation**: All public APIs have XML docs
- [ ] **Naming**: Follows conventions (PascalCase, descriptive)
- [ ] **Math**: Uses `Unity.Mathematics` (float3, int2, math.*)
- [ ] **No GC**: No managed types in hot paths

### Test Prompts

Try these prompts to verify Copilot is following the rules:

1. **Component Test**:
   ```
   Create a GridPosition component for tile-based positioning
   ```
   ? Should generate: Unmanaged struct with `int2` field

2. **System Test**:
   ```
   Create a system to update velocities with gravity
   ```
   ? Should include: `[BurstCompile]`, proper update group, XML docs

3. **Aspect Test**:
   ```
   Create a MovementAspect combining transform and velocity
   ```
   ? Should generate: Readonly partial struct with IAspect

4. **Utility Test**:
   ```
   Create a grid utility to convert world to grid coordinates
   ```
   ? Should use: `float3`, `int2`, `math.*`, `[BurstCompile]`

---

## ?? Best Practices

### When Writing Prompts

**DO**:
- ? Be specific: "Create a Burst-compiled system for X"
- ? Mention context: "In the Transport context, create..."
- ? Reference patterns: "Following the aspect pattern, create..."
- ? Specify performance: "This will run on 10k entities"

**DON'T**:
- ? Be vague: "Create a system"
- ? Skip context: "Add some code"
- ? Ignore scale: "Make it work"

### Example Prompts (Copy & Paste)

```
# Components
"Create a Health component with current and max values following component guidelines"
"Create an enableable component for marking selected entities"

# Systems
"Create a Burst-compiled system in FixedStepSimulationSystemGroup to apply gravity"
"Add a system to process spawner requests using EntityCommandBuffer"

# Aspects
"Create a BuildingAspect for managing construction state and validation"
"Add a PathfindingAspect with A* pathfinding logic"

# Utilities
"Create grid utilities for neighbor queries with Burst support"
"Add math utilities for bezier curve interpolation using Unity.Mathematics"

# Converters
"Create authoring and baker for track segments with start/end transforms"
"Add a spawner authoring component with prefab reference and spawn rate"
```

---

## ?? Customization

### Adjusting Rules

1. **Edit** `.github/copilot-instructions.md` for project-wide changes
2. **Edit** `Assets/Scripts/[Folder]/.copilot-instructions.md` for domain changes
3. **Commit** changes to version control
4. **Restart** VS Code/Visual Studio (Copilot reloads instructions)

### Adding New Bounded Contexts

When you create new folders like `Assets/Scripts/World/`:

1. **Copy** `.github/copilot-instructions-template.md`
2. **Rename** to `Assets/Scripts/World/.copilot-instructions.md`
3. **Fill in** context-specific rules and patterns
4. **Document** dependencies and integration points

---

## ?? Learning Path

### Week 1: Fundamentals
- [ ] Read `.github/copilot-instructions.md` (30 min)
- [ ] Try 5-10 component generation prompts (1 hour)
- [ ] Create 2-3 simple systems with Copilot (2 hours)
- [ ] Review generated code against guidelines (1 hour)

### Week 2: Advanced Patterns
- [ ] Read all domain-specific instructions (1 hour)
- [ ] Create aspects for complex behaviors (2 hours)
- [ ] Add authoring components and bakers (2 hours)
- [ ] Write Burst-compatible utilities (1 hour)

### Week 3: Integration
- [ ] Build a complete feature using Copilot (4 hours)
- [ ] Review and refactor generated code (2 hours)
- [ ] Profile and optimize (2 hours)
- [ ] Document learnings (30 min)

---

## ?? Troubleshooting

### Copilot Not Following Instructions

**Problem**: Generated code doesn't match patterns

**Solutions**:
1. Check file location (Copilot loads folder-specific instructions)
2. Be more explicit in prompts (mention "following Burst guidelines")
3. Restart IDE (instructions reload on startup)
4. Check for conflicting instructions

### Generated Code Has Issues

**Problem**: Code compiles but doesn't follow best practices

**Solutions**:
1. Ask for refactoring: "Refactor this to use Burst compilation"
2. Reference specific pattern: "Update to match the IJobEntity pattern"
3. Request performance improvements: "Optimize for 10k entities"

### Missing Patterns

**Problem**: Needed pattern not documented

**Solutions**:
1. Add to appropriate `.copilot-instructions.md`
2. Use template as guide
3. Document with examples
4. Commit for team sharing

---

## ?? Metrics to Track

### Code Quality
- [ ] All components are unmanaged
- [ ] All systems are Burst-compiled
- [ ] Zero GC allocations in hot paths
- [ ] XML documentation coverage > 90%

### Performance
- [ ] Frame rate: 60 FPS with target entity count
- [ ] System update time: < 1ms per system
- [ ] Memory layout: Contiguous (NativeArray usage)

### Productivity
- [ ] Time to implement feature (before/after Copilot)
- [ ] Code review issues (reduce boilerplate issues)
- [ ] Documentation completeness (auto-generated XML docs)

---

## ?? Additional Resources

### Unity Documentation
- [DOTS Best Practices](https://docs.unity3d.com/Packages/com.unity.entities@latest)
- [Burst Compiler](https://docs.unity3d.com/Packages/com.unity.burst@latest)
- [Entities Graphics](https://docs.unity3d.com/Packages/com.unity.entities.graphics@latest)
- [Unity Mathematics](https://docs.unity3d.com/Packages/com.unity.mathematics@latest)

### Internal Documentation
- `.github/copilot-instructions.md` - Main guidelines
- `COPILOT-INSTRUCTIONS-README.md` - Quick reference
- Domain-specific `.copilot-instructions.md` files

---

## ? Next Steps

1. **? Verify installation** (see commands above)
2. **?? Read** `.github/copilot-instructions.md` 
3. **?? Test** with sample prompts
4. **?? Start building** your first feature
5. **?? Document** learnings and update instructions
6. **?? Share** with team

---

**Installation Date**: 2025-01-01  
**Unity Version**: 6.5 (Alpha)  
**DOTS Version**: 1.x  
**Total Documentation**: ~2,550 lines across 10 files  

**Happy Coding with Copilot! ?????**
