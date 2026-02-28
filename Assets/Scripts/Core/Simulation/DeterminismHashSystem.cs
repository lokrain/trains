#nullable enable
using Unity.Burst;
using Unity.Entities;

namespace OpenTTD.Core.Simulation
{
    /// <summary>
    /// Periodic deterministic state hash publication scaffold.
    /// </summary>
    public struct DeterminismHashState : IComponentData
    {
        public ulong LastPublishedTick;
        public ulong LastHash;
        public uint PublishIntervalTicks;
    }

    /// <summary>
    /// Creates deterministic hash state singleton.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct DeterminismHashBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            EntityQuery query = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<DeterminismHashState>());
            if (query.IsEmptyIgnoreFilter)
            {
                Entity entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(entity, new DeterminismHashState
                {
                    LastPublishedTick = 0,
                    LastHash = 0,
                    PublishIntervalTicks = 60
                });
            }
        }

        public readonly void OnUpdate(ref SystemState state)
        {
        }
    }

    /// <summary>
    /// Publishes canonical hash samples every configured interval.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PostSimGroup))]
    public partial struct DeterminismHashPublishSystem : ISystem
    {
        [BurstCompile]
        public readonly void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimTickState>();
            state.RequireForUpdate<DeterminismHashState>();
        }

        [BurstCompile]
        public readonly void OnUpdate(ref SystemState state)
        {
            Entity tickEntity = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<SimTickState>()).GetSingletonEntity();
            Entity hashEntity = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<DeterminismHashState>()).GetSingletonEntity();

            SimTickState tick = state.EntityManager.GetComponentData<SimTickState>(tickEntity);
            DeterminismHashState hashState = state.EntityManager.GetComponentData<DeterminismHashState>(hashEntity);

            uint interval = hashState.PublishIntervalTicks == 0 ? 1u : hashState.PublishIntervalTicks;

            if (tick.Tick % interval != 0)
            {
                return;
            }

            ulong hash = Seed(hashState.LastHash);
            hash = Combine(hash, tick.Tick);
            hash = Combine(hash, interval);

            hashState.LastHash = hash;
            hashState.LastPublishedTick = tick.Tick;
            state.EntityManager.SetComponentData(hashEntity, hashState);
        }

        [BurstCompile]
        private static ulong Seed(ulong value)
        {
            return Mix(value ^ 0xCBF29CE484222325ul);
        }

        [BurstCompile]
        private static ulong Combine(ulong current, ulong value)
        {
            return Mix(current ^ (value + 0x9E3779B97F4A7C15ul + (current << 6) + (current >> 2)));
        }

        [BurstCompile]
        private static ulong Combine(ulong current, uint value)
        {
            return Combine(current, (ulong)value);
        }

        [BurstCompile]
        private static ulong Mix(ulong x)
        {
            x ^= x >> 30;
            x *= 0xBF58476D1CE4E5B9ul;
            x ^= x >> 27;
            x *= 0x94D049BB133111EBul;
            x ^= x >> 31;
            return x;
        }
    }
}
