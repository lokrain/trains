# [Bounded Context Name] - Copilot Instructions Template

> **Copy this template when creating a new bounded context folder**
> Replace `[Bounded Context Name]` with your context (e.g., World, Transport, Vehicles)

## Purpose
[Brief description of this bounded context's responsibility]

Example:
> This folder contains all code related to the **World** bounded context, including terrain generation, tile management, chunk streaming, and level-of-detail systems.

## Bounded Context Rules

### Responsibilities
What this context **owns**:
- [Responsibility 1]
- [Responsibility 2]
- [Responsibility 3]

### Dependencies
What this context **depends on**:
- [Context 1]: [Why and how]
- [Context 2]: [Why and how]

### Consumers
Who **depends on** this context:
- [Context 1]: [What they use]
- [Context 2]: [What they use]

### Boundaries
What this context **does NOT** handle:
- [Responsibility that belongs elsewhere]
- [Responsibility that belongs elsewhere]

## Components

### Core Components
List the key components in this context:

```csharp
// Example:
public struct TerrainTile : IComponentData {
    public int2 GridPosition;
    public TileType Type;
    public float Height;
}
```

### Singleton Components
```csharp
// Example:
public struct TerrainConfig : IComponentData {
    public int ChunkSize;
    public float NoiseScale;
    public int Seed;
}
```

### Buffer Components
```csharp
// Example:
public struct ChunkTile : IBufferElementData {
    public Entity TileEntity;
}
```

## Systems

### Core Systems
List the main systems and their responsibilities:

1. **[SystemName]**
   - **Purpose**: [What it does]
   - **Update Group**: `[GroupName]`
   - **Update Order**: [Before/After what]
   - **Performance**: [Expected entity count, timing]

Example:
```csharp
/// <summary>
/// Generates terrain chunks based on player position.
/// </summary>
[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct ChunkGenerationSystem : ISystem {
    // Implementation...
}
```

## Aspects

### Core Aspects
List the main aspects and their purpose:

```csharp
// Example:
/// <summary>
/// Provides terrain tile manipulation functionality.
/// </summary>
public readonly partial struct TileAspect : IAspect {
    readonly RefRW<TerrainTile> tile;
    readonly RefRO<LocalTransform> transform;
    
    public void UpdateHeight(float newHeight) {
        tile.ValueRW.Height = newHeight;
    }
}
```

## Domain-Specific Patterns

### [Pattern Name 1]
```csharp
// Example: Chunk Streaming Pattern
// Show common patterns specific to this domain
```

### [Pattern Name 2]
```csharp
// Example: LOD Management Pattern
// Show common patterns specific to this domain
```

## Integration Points

### Events/Messages
How this context communicates with others:

```csharp
// Example:
public struct ChunkLoadedEvent : IComponentData {
    public Entity ChunkEntity;
    public int2 ChunkCoordinate;
}
```

### Singleton Services
Services exposed to other contexts:

```csharp
// Example:
public struct TerrainQueryService : IComponentData {
    public Entity GridEntity;
    
    // Query methods via systems
}
```

## Performance Considerations

### Expected Scale
- Entity count: [X - Y entities]
- Update frequency: [Every frame / Fixed step / On demand]
- Memory budget: [Rough estimate]

### Optimization Strategies
- [Strategy 1]: [Why and when]
- [Strategy 2]: [Why and when]

### Profiling Targets
- [System 1]: < [X]ms per frame
- [System 2]: < [X]ms per frame

## Testing Strategy

### Unit Tests
What to test at the component/utility level:
- [Test case 1]
- [Test case 2]

### Integration Tests
What to test at the system level:
- [Test case 1]
- [Test case 2]

### Performance Tests
- [Performance benchmark 1]
- [Performance benchmark 2]

## Common Anti-Patterns in This Context

### ? Anti-Pattern 1
```csharp
// ? Bad: [Description]
// Code example showing what NOT to do
```

### ? Correct Pattern
```csharp
// ? Good: [Description]
// Code example showing the right way
```

## Resources & References

### External Documentation
- [Link to Unity docs]
- [Link to research papers, if applicable]

### Internal Documentation
- [Link to design docs]
- [Link to architecture diagrams]

### Examples
- [Link to example scenes]
- [Link to test cases]

---

## Template Instructions

When creating a new bounded context instruction file:

1. **Copy this template** to `Assets/Scripts/[ContextName]/.copilot-instructions.md`
2. **Replace all `[placeholders]`** with actual context information
3. **Add domain-specific patterns** that are unique to this context
4. **Document integration points** with other contexts
5. **Define performance targets** based on expected scale
6. **Add examples** from your actual codebase
7. **Update the main README** to reference the new context

### Example Contexts

#### World Context
- Terrain, tiles, chunks, streaming, LOD
- Dependencies: None (foundation layer)
- Consumers: Build, Transport, Economy

#### Transport Context
- Tracks, roads, pathfinding, signals, stations
- Dependencies: World (queries terrain)
- Consumers: Vehicles, Build

#### Vehicles Context
- Trains, buses, movement, schedules
- Dependencies: Transport (uses tracks)
- Consumers: Economy, UI

#### Build Context
- Player tools, placement rules, undo/redo
- Dependencies: World, Transport (validates placement)
- Consumers: UI

#### Economy Context
- Companies, cashflow, cargo, towns, industry
- Dependencies: Vehicles, Transport (tracks cargo)
- Consumers: UI

---

**Last Updated**: 2025-01-01
**Unity Version**: 6.5 (Alpha)
**DOTS Version**: 1.x
