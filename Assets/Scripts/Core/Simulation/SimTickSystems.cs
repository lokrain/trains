#nullable enable
using Unity.Burst;
using Unity.Entities;

namespace OpenTTD.Core.Simulation
{
    /// <summary>
    /// Ensures simulation tick singleton exists.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public partial struct SimTickBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<SimTickState>())
            {
                Entity entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(entity, new SimTickState
                {
                    Tick = 0,
                    FixedDeltaTime = 1f / 30f,
                    MaxCatchUpSteps = 4
                });
            }
        }

        public void OnUpdate(ref SystemState state)
        {
        }
    }

    /// <summary>
    /// Advances authoritative simulation tick in fixed-step group.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimTickGroup), OrderFirst = true)]
    public partial struct SimTickAdvanceSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimTickState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            RefRW<SimTickState> simTick = SystemAPI.GetSingletonRW<SimTickState>();
            simTick.ValueRW.Tick += 1;
            simTick.ValueRW.FixedDeltaTime = SystemAPI.Time.DeltaTime;
            if (simTick.ValueRW.MaxCatchUpSteps <= 0)
            {
                simTick.ValueRW.MaxCatchUpSteps = 4;
            }
        }
    }
}
