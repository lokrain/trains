# Copilot Instructions - Quick Reference

This document provides an overview of all Copilot instruction files in this project and quick links to key patterns.

## Instruction Files

### Root Instructions
- **[.github/copilot-instructions.md](.github/copilot-instructions.md)** - Main project-wide instructions covering:
  - Non-negotiables (performance, data-oriented design, Burst, zero GC)
  - Architecture rules (bounded contexts, deterministic simulation)
  - Code style and documentation standards
  - Feature delivery protocol
  - Common patterns and anti-patterns

### Domain-Specific Instructions

| Folder | Instruction File | Purpose |
|--------|-----------------|---------|
| `Assets/Scripts/Components/` | [.copilot-instructions.md](Assets/Scripts/Components/.copilot-instructions.md) | Pure data components, naming, patterns |
| `Assets/Scripts/Systems/` | [.copilot-instructions.md](Assets/Scripts/Systems/.copilot-instructions.md) | System design, jobs, queries, performance |
| `Assets/Scripts/Aspects/` | [.copilot-instructions.md](Assets/Scripts/Aspects/.copilot-instructions.md) | Aspect patterns, encapsulation, usage |
| `Assets/Scripts/Utilities/` | [.copilot-instructions.md](Assets/Scripts/Utilities/.copilot-instructions.md) | Utilities, helpers, builders, extensions |

## Quick Pattern Reference

### Creating a Component
```csharp
/// <summary>
/// Represents the velocity of an entity in meters per second.
/// </summary>
public struct Velocity : IComponentData {
    public float3 Value;
}
```

### Creating a System
```csharp
/// <summary>
/// Updates entity positions based on velocity.
/// </summary>
[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct MovementSystem : ISystem {
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var deltaTime = SystemAPI.Time.DeltaTime;
        
        foreach (var (transform, velocity) in 
                 SystemAPI.Query<RefRW<LocalTransform>, RefRO<Velocity>>()) {
            transform.ValueRW.Position += velocity.ValueRO.Value * deltaTime;
        }
    }
}
```

### Creating an Aspect
```csharp
/// <summary>
/// Provides movement functionality for vehicles.
/// </summary>
public readonly partial struct VehicleAspect : IAspect {
    readonly RefRW<LocalTransform> transform;
    readonly RefRO<Velocity> velocity;
    readonly RefRO<Speed> speed;
    
    public void Move(float deltaTime) {
        var direction = math.normalize(velocity.ValueRO.Value);
        var distance = speed.ValueRO.Value * deltaTime;
        transform.ValueRW.Position += direction * distance;
    }
}
```

### Creating a Utility
```csharp
/// <summary>
/// Grid coordinate conversion utilities.
/// </summary>
public static class GridUtilities {
    /// <summary>
    /// Converts world position to grid coordinates.
    /// </summary>
    [BurstCompile]
    public static int2 WorldToGrid(float3 worldPos, float cellSize) {
        return new int2(
            (int)math.floor(worldPos.x / cellSize),
            (int)math.floor(worldPos.z / cellSize)
        );
    }
}
```

## Key Principles Cheat Sheet

### ? DO
- Use **unmanaged types** in components (`float3`, `int2`, `FixedString64Bytes`)
- Mark systems with **`[BurstCompile]`**
- Use **`IJobEntity`** for parallel processing
- Keep systems **stateless** (dependencies via singletons)
- Use **`EntityCommandBuffer`** for structural changes
- Use **`IEnableableComponent`** for toggleable states
- Prefer **`RefRO<T>`** for read-only access
- Document **all public APIs** with XML comments
- Use **Unity.Mathematics** (`float3`, `math.*`)

### ? DON'T
- Use managed types in components (`string`, `List<T>`, `class`)
- Add/remove components in hot loops (use ECB instead)
- Store state in systems (use components/singletons)
- Use `UnityEngine.Mathf` (use `Unity.Mathematics.math` instead)
- Use `UnityEngine.Random` (use `Unity.Mathematics.Random` instead)
- Use LINQ in hot paths (allocates garbage)
- Nest queries (O(n²) - use spatial partitioning)
- Access `EntityManager` directly in jobs (use ECB)

## Performance Targets

| Metric | Target |
|--------|--------|
| Frame rate | 60 FPS |
| Entity count | 10k-50k entities |
| System update time | <1ms per system |
| GC allocations | Near-zero per frame |
| Memory layout | Contiguous (NativeArray, NativeList) |

## Naming Conventions

| Type | Convention | Example |
|------|-----------|---------|
| Component | `PascalCase` noun | `Velocity`, `GridPosition` |
| System | `PascalCase` verb+noun+System | `MovementSystem`, `PathfindingSystem` |
| Job | `PascalCase` verb+Job | `UpdateVelocityJob`, `CalculatePathJob` |
| Aspect | `PascalCase` noun+Aspect | `VehicleAspect`, `TransformAspect` |
| Utility | `PascalCase` noun+Utilities | `GridUtilities`, `MathUtilities` |
| Constants | `PascalCase` | `MaxSpeed`, `DefaultCellSize` |
| Variables | `camelCase` | `deltaTime`, `entityManager` |

## Update Groups

| Group | Purpose | Example Systems |
|-------|---------|-----------------|
| `InitializationSystemGroup` | Setup, initialization | `LoadConfigSystem`, `InitializeGridSystem` |
| `FixedStepSimulationSystemGroup` | Deterministic simulation | `PhysicsSystem`, `MovementSystem` |
| `SimulationSystemGroup` | Variable timestep simulation | `InputSystem`, `CollisionSystem` |
| `PresentationSystemGroup` | Rendering, UI | `RenderSystem`, `UIUpdateSystem` |

## Common Attributes

```csharp
[BurstCompile]                                    // Enable Burst compilation
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]  // Set update group
[UpdateBefore(typeof(OtherSystem))]              // Order before system
[UpdateAfter(typeof(OtherSystem))]               // Order after system
[Optional]                                        // Optional component in aspect
[ReadOnly]                                        // Read-only component lookup
[DeallocateOnJobCompletion]                      // Auto-dispose after job
```

## Bounded Contexts (Planned Architecture)

```
Assets/Scripts/
??? World/              # Terrain, tiles, chunks, streaming, LOD
??? Build/              # Player tools, placement rules, undo/redo
??? Transport/          # Tracks/roads, pathfinding, signals, stations
??? Vehicles/           # Trains, buses, movement, schedules
??? Economy/            # Companies, cashflow, cargo, towns, industry
??? UI/                 # Unity UI Toolkit + thin adapters to simulation
??? SaveLoad/           # Versioned binary serialization + replay
??? Core/               # Shared utilities, math, constants
??? Debug/              # Visualization, gizmos, dev tools
```

## Resources

- [DOTS Best Practices](https://docs.unity3d.com/Packages/com.unity.entities@latest)
- [Burst Compiler](https://docs.unity3d.com/Packages/com.unity.burst@latest)
- [Entities Graphics](https://docs.unity3d.com/Packages/com.unity.entities.graphics@latest)
- [Unity Mathematics](https://docs.unity3d.com/Packages/com.unity.mathematics@latest)

## Getting Started with Copilot

When asking Copilot for help:

1. **Be specific** about the bounded context: "Create a component in the Transport context"
2. **Mention performance requirements**: "This will run on 10k entities"
3. **Reference patterns**: "Use the pattern from Components/.copilot-instructions.md"
4. **Ask for complete files**: "Provide the complete file, not a snippet"

Example prompts:
- "Create a new VehicleMovement component following the component guidelines"
- "Add a Burst-compiled system to update train positions along tracks"
- "Create an aspect for managing building construction state"
- "Add grid utilities for neighbor queries with Burst support"

---

**Last Updated**: 2025-01-01
**Unity Version**: 6.5 (Alpha)
**DOTS Version**: 1.x
