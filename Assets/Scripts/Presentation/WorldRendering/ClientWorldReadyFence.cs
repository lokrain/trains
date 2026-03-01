#nullable enable
using Unity.Entities;

namespace OpenTTD.Presentation.WorldRendering
{
    /// <summary>
    /// Client-side readiness fence used to gate initial presenter creation until join data is ready.
    /// </summary>
    public struct ClientWorldReadyFence : IComponentData
    {
        public byte IsReady;
    }

    /// <summary>
    /// Ensures a world-ready fence singleton exists.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class ClientWorldReadyFenceBootstrapSystem : SystemBase
    {
        protected override void OnStartRunning()
        {
            if (SystemAPI.TryGetSingletonEntity<ClientWorldReadyFence>(out _))
            {
                return;
            }

            Entity e = EntityManager.CreateEntity(typeof(ClientWorldReadyFence));
            EntityManager.SetComponentData(e, new ClientWorldReadyFence { IsReady = 0 });
        }

        protected override void OnUpdate()
        {
        }
    }
}
