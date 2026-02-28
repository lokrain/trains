# City Builder TDD - Copilot Instructions

> **OpenTTD-like transport city builder** in **Unity 6.5 (Alpha)** using **ECS/DOTS + Burst + Entities Graphics (URP)**.

## 0) Non-Negotiables (Quality + Performance)

### Data-Oriented First
- **Store large world/sim data in contiguous memory**: Use `NativeArray`, `NativeList`, `NativeHashMap`, blob assets
- **Avoid per-tile/per-entity managed allocations**: No `List<T>`, `Dictionary<T>`, `class` in hot paths
- **Prefer blobs for read-only data**: Terrain configs, vehicle stats, building definitions

### Burst Everywhere It Matters
- **Any hot loop must be Burst-compatible**: Use `IJobEntity`, `IJobChunk`, `IJobParallelFor`
- **Use Unity.Mathematics**: `float3`, `int2`, `quaternion` instead of `Vector3`, `Vector2Int`, `Quaternion`
- **Mark systems with `[BurstCompile]`**: Enable Burst for `ISystem` implementations
- **No managed references in jobs**: Pass indices/entity IDs instead of object references

### No GC Spikes
- **Near-zero allocations per frame**: Profile with Unity Profiler to verify
- **Avoid LINQ/closures in hot paths**: Use explicit loops with Burst
- **Dispose native containers**: Use `[DeallocateOnJobCompletion]` or manual disposal in `OnDestroy`
- **String allocations**: Cache strings, use `FixedString` in components

### Profile Before "Optimizing"
- **Only optimize after measuring**: Use Unity Profiler (CPU, Memory, Deep Profile)
- **Write micro-benchmarks when needed**: Use Unity Performance Testing package
- **Capture baselines**: Document performance targets (60 FPS with 10k entities, etc.)

### Avoid Structural Changes in Hot Loops
- **Batch changes via ECB**: `EntityCommandBuffer` or `EntityCommandBufferSystem`
- **Use state components/tags**: Prefer `IEnableableComponent` or state flags over add/remove
- **Defer entity creation/destruction**: Collect requests, process in batches

### Rendering (URP + Entities Graphics)
- **Use Entities Graphics for large-scale instancing**: RegisterMaterialOverride, BatchRendererGroup
- **Leverage GPU instancing**: Per-instance material properties via components
- **URPMaterialPropertyBaseColor, URPMaterialPropertyEmissionColor**: Built-in override components
- **Hybrid Renderer V2**: Use `RenderMeshArray` and `MaterialMeshInfo`

---

## 1) Architecture Rules (DDD-ish, Practical for DOTS)

### Bounded Contexts (Folders + Assembly Definitions)

Organize code into these **bounded contexts**:

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

#### Explicit Boundaries
- Each context has its own **assembly definition** (`.asmdef`)
- Cross-context dependencies are **explicit and one-way**:
  - `Vehicles` ? `Transport` (vehicles use tracks)
  - `Build` ? `World` (placement queries terrain)
  - `Economy` ? `Vehicles`, `Transport` (tracks cashflow)
  - `UI` ? all contexts (read-only queries)
- **No circular dependencies**: Use events/singletons for loose coupling

### Deterministic Simulation
- **Fixed-step tick**: `[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]`
- **Explicit random streams**: `Random` component with seed, not `UnityEngine.Random`
- **Versioned save schema**: Include schema version in save files
- **Deterministic replay**: Optional event sourcing for multiplayer/debugging

---

## 2) How Copilot Should Code (Style)

### Components

#### Small & Composable
```csharp
// ? Good: Small, focused components
public struct Position : IComponentData { public float3 Value; }
public struct Velocity : IComponentData { public float3 Value; }
public struct Speed : IComponentData { public float Value; }

// ? Bad: Monolithic component
public struct VehicleData : IComponentData {
    public float3 Position;
    public float3 Velocity;
    public float Speed;
    public float Health;
    public int CargoAmount;
    // ... 20 more fields
}
```

#### Pure Data (Unmanaged)
```csharp
// ? Good: Unmanaged, Burst-compatible
public struct TrackSegment : IComponentData {
    public Entity StartNode;
    public Entity EndNode;
    public float Length;
    public TrackType Type;
}

// ? Bad: Managed fields
public struct TrackSegment : IComponentData {
    public string Name;              // ? Managed string
    public List<Entity> Nodes;       // ? Managed collection
}
```

#### Use Enableable Components for State
```csharp
// ? Good: Toggleable state without structural changes
public struct IsMoving : IComponentData, IEnableableComponent { }

// In system:
SystemAPI.SetComponentEnabled<IsMoving>(entity, velocity.Value.LengthSq() > 0.01f);
```

#### Blob Assets for Read-Only Data
```csharp
// ? Good: Large read-only data
public struct VehicleDefinition {
    public BlobArray<float3> MeshVertices;
    public BlobArray<int> MeshIndices;
    public float MaxSpeed;
    public float Acceleration;
}

public struct VehicleDefinitionRef : IComponentData {
    public BlobAssetReference<VehicleDefinition> Value;
}
```

### Systems

#### Stateless & Dependency Injection
```csharp
// ? Good: Stateless system, singleton dependencies
[BurstCompile]
public partial struct TrainMovementSystem : ISystem {
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var config = SystemAPI.GetSingleton<SimulationConfig>();
        var deltaTime = SystemAPI.Time.DeltaTime;
        
        foreach (var (transform, velocity) in 
                 SystemAPI.Query<RefRW<LocalTransform>, RefRO<Velocity>>()) {
            transform.ValueRW.Position += velocity.ValueRO.Value * deltaTime;
        }
    }
}

// ? Bad: Stateful system with mutable fields
public partial struct TrainMovementSystem : ISystem {
    private float accumulatedTime;  // ? State in system
    private NativeList<Entity> pendingTrains; // ? Unmanaged state
}
```

#### Use Jobs for Parallelism
```csharp
// ? Good: Parallel job
[BurstCompile]
public partial struct UpdateVelocitySystem : ISystem {
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var job = new VelocityJob { DeltaTime = SystemAPI.Time.DeltaTime };
        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct VelocityJob : IJobEntity {
    public float DeltaTime;
    
    void Execute(ref LocalTransform transform, in Velocity velocity) {
        transform.Position += velocity.Value * DeltaTime;
    }
}
```

### Documentation

#### XML Docs + Usage Example
```csharp
/// <summary>
/// Calculates the shortest path between two track nodes using A* algorithm.
/// </summary>
/// <param name="start">Starting track node entity</param>
/// <param name="goal">Goal track node entity</param>
/// <param name="path">Resulting path as a list of track segment entities</param>
/// <returns>True if a path was found, false otherwise</returns>
/// <example>
/// <code>
/// var pathfinder = new TrackPathfinder(ref state);
/// if (pathfinder.FindPath(startNode, goalNode, out var path)) {
///     // Use path...
///     path.Dispose();
/// }
/// </code>
/// </example>
public bool FindPath(Entity start, Entity goal, out NativeList<Entity> path) {
    // Implementation...
}
```

### Testing

```csharp
[TestFixture]
public class PlacementRulesTests {
    [Test]
    public void CanPlaceTrack_OnFlatTerrain_ReturnsTrue() {
        // Arrange
        using var world = new World("Test World");
        var manager = world.EntityManager;
        var terrainEntity = CreateFlatTerrain(manager, 10, 10);
        
        // Act
        var canPlace = PlacementRules.CanPlaceTrack(manager, terrainEntity, new int2(5, 5));
        
        // Assert
        Assert.IsTrue(canPlace);
    }
}
```

---

## 3) Feature Delivery Protocol

When implementing a story, follow this order:

### 1. Create/Adjust Data Model
- Define components (pure data structs)
- Define blob assets for read-only data
- Define singleton components for global state

### 2. Implement System(s) with Burst + Jobs
- Create `ISystem` or `SystemBase` (prefer `ISystem`)
- Use `IJobEntity`, `IJobChunk`, or `IJobParallelFor`
- Mark with `[BurstCompile]` and verify compilation

### 3. Add Debug Visualization + Minimal UI Hook
- Gizmos for in-scene visualization
- Hotkeys for testing (e.g., `F1` to spawn train)
- Minimal UI Toolkit panel if needed

### 4. Add Instrumentation
- Counters: `EntityCommandBufferSystem.Singleton.PendingBuffers.Length`
- Timings: `System.Time.ElapsedTime`, `Stopwatch` for profiling
- Asserts: `Assert.IsTrue(velocity.Value.IsFinite())`

### 5. Add Tests
- EditMode tests for pure logic (pathfinding, placement rules)
- PlayMode tests for system integration

### 6. Update Docs + Changelog
- Update README or feature docs
- Add entry to CHANGELOG.md with format: `[Added] Feature description`

---

## 4) What to Ask When Requirements Are Unclear

Ask **one high-leverage question**:

> "Is this feature required for the **vertical slice** (Map + camera + build rails + train runs) or post-slice?"

**Vertical Slice Priorities:**
1. ? Terrain rendering + camera controls
2. ? Track placement tool
3. ? Train spawning + movement along tracks
4. ? Basic UI (build menu, info panel)
5. ?? Economy/cargo (post-slice)
6. ?? Signals/pathfinding (post-slice)
7. ?? Save/load (post-slice)

---

## 5) Deliverables Expectations

### Complete Files, Not Snippets
- Provide full file content with all necessary `using` statements
- Include namespace declarations
- Show complete class/struct definitions

### Commands to Validate
```bash
# Build project
dotnet build Assembly-CSharp.csproj

# Run tests
# (In Unity Editor: Window > General > Test Runner)

# Profile
# (In Unity Editor: Window > Analysis > Profiler)
```

### Acceptance Criteria
Before coding, confirm:
- [ ] Data model defined (components, blobs, singletons)
- [ ] Systems identified with update order
- [ ] Performance targets defined (e.g., 60 FPS with 10k entities)
- [ ] Test coverage plan (unit tests for logic, play mode for integration)
- [ ] UI/debug hooks identified

---

## 6) Code Style

### Naming Conventions
- **Components**: `PascalCase`, noun (e.g., `Velocity`, `TrackSegment`)
- **Systems**: `PascalCase`, verb + noun + "System" (e.g., `TrainMovementSystem`)
- **Jobs**: `PascalCase`, verb + "Job" (e.g., `CalculatePathJob`)
- **Variables**: `camelCase` (e.g., `deltaTime`, `entityManager`)
- **Constants**: `PascalCase` (e.g., `MaxTrackLength`)

### Formatting
- **Indentation**: 4 spaces (no tabs)
- **Braces**: Allman style (opening brace on new line)
- **Line length**: 120 characters max
- **Spacing**: One blank line between members

### Comments
- **XML docs**: For all public APIs
- **Inline comments**: Only for complex logic or non-obvious workarounds
- **TODO comments**: Format as `// TODO(username): Description`

---

## 7) Common Patterns

### Entity Creation
```csharp
// ? Good: Batch creation with ECB
var ecb = new EntityCommandBuffer(Allocator.TempJob);
for (int i = 0; i < count; i++) {
    var entity = ecb.CreateEntity();
    ecb.AddComponent(entity, new Position { Value = positions[i] });
    ecb.AddComponent(entity, new Velocity { Value = velocities[i] });
}
ecb.Playback(state.EntityManager);
ecb.Dispose();
```

### Singleton Access
```csharp
// ? Good: Singleton component
var config = SystemAPI.GetSingleton<SimulationConfig>();

// ? Good: Managed singleton
var pathfinder = World.GetExistingSystemManaged<PathfindingSystem>();
```

### Queries
```csharp
// ? Good: Efficient query with aspect
var trainAspect = SystemAPI.GetAspect<TrainAspect>(entity);
trainAspect.Move(deltaTime);

// ? Good: Parallel query
foreach (var (transform, velocity) in 
         SystemAPI.Query<RefRW<LocalTransform>, RefRO<Velocity>>()
                   .WithAll<IsMoving>()) {
    // Process...
}
```

### Cleanup
```csharp
[BurstCompile]
public partial struct MySystem : ISystem {
    private NativeList<Entity> buffer;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        buffer = new NativeList<Entity>(Allocator.Persistent);
    }
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state) {
        if (buffer.IsCreated) buffer.Dispose();
    }
}
```

---

## 8) Anti-Patterns to Avoid

### ? Structural Changes in Hot Loops
```csharp
// ? Bad: Adding/removing components every frame
foreach (var (entity, velocity) in SystemAPI.Query<RefRO<Velocity>>().WithEntityAccess()) {
    if (velocity.ValueRO.Value.LengthSq() > 0) {
        state.EntityManager.AddComponent<IsMoving>(entity);
    } else {
        state.EntityManager.RemoveComponent<IsMoving>(entity);
    }
}

// ? Good: Use enableable component
foreach (var (entity, velocity) in SystemAPI.Query<RefRO<Velocity>>().WithEntityAccess()) {
    var isMoving = velocity.ValueRO.Value.LengthSq() > 0;
    SystemAPI.SetComponentEnabled<IsMoving>(entity, isMoving);
}
```

### ? Managed Collections in Components
```csharp
// ? Bad: Managed collection
public struct Train : IComponentData {
    public List<Entity> Carriages;  // ? Can't Burst compile
}

// ? Good: DynamicBuffer
public struct CarriageElement : IBufferElementData {
    public Entity Value;
}
// Add buffer: state.EntityManager.AddBuffer<CarriageElement>(trainEntity);
```

### ? LINQ in Hot Paths
```csharp
// ? Bad: LINQ allocates
var activeTrains = trains.Where(t => t.IsActive).ToList();

// ? Good: Explicit loop with native container
var activeTrains = new NativeList<Entity>(Allocator.Temp);
foreach (var train in trains) {
    if (train.IsActive) activeTrains.Add(train.Entity);
}
```

---

## 9) Resources

- [DOTS Best Practices](https://docs.unity3d.com/Packages/com.unity.entities@latest)
- [Burst Compiler](https://docs.unity3d.com/Packages/com.unity.burst@latest)
- [Entities Graphics](https://docs.unity3d.com/Packages/com.unity.entities.graphics@latest)
- [Unity Mathematics](https://docs.unity3d.com/Packages/com.unity.mathematics@latest)

---

**Last Updated**: 2025-01-01
**Unity Version**: 6.5 (Alpha)
**DOTS Version**: 1.x
