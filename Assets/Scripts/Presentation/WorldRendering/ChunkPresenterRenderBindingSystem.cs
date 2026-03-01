#nullable enable
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.Rendering;

namespace OpenTTD.Presentation.WorldRendering
{
    /// <summary>
    /// Attaches Entities Graphics render bindings to presenter entities that are pending setup.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(ChunkPresenterLifecycleSystem))]
    public partial class ChunkPresenterRenderBindingSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonEntity<ChunkPresenterRenderResources>(out Entity resourcesEntity))
            {
                return;
            }

            if (!EntityManager.HasComponent<RenderMeshArray>(resourcesEntity))
            {
                return;
            }

            RenderMeshArray renderMeshArray = EntityManager.GetSharedComponentManaged<RenderMeshArray>(resourcesEntity);
            ChunkPresenterRenderResources resources = EntityManager.GetComponentData<ChunkPresenterRenderResources>(resourcesEntity);
            MaterialMeshInfo materialMeshInfo = MaterialMeshInfo.FromRenderMeshArrayIndices(resources.MaterialIndex, resources.MeshIndex);
            var desc = new RenderMeshDescription(
                shadowCastingMode: resources.CastShadows != 0 ? ShadowCastingMode.On : ShadowCastingMode.Off,
                receiveShadows: resources.ReceiveShadows != 0);

            var toBind = new NativeList<Entity>(Allocator.Temp);
            foreach (var (_, entity) in SystemAPI.Query<RefRO<ChunkRenderBindingPending>>().WithAll<ChunkRenderTag>().WithEntityAccess())
            {
                toBind.Add(entity);
            }

            for (int i = 0; i < toBind.Length; i++)
            {
                Entity entity = toBind[i];

                if (!EntityManager.HasComponent<MaterialMeshInfo>(entity))
                {
                    RenderMeshUtility.AddComponents(entity, EntityManager, desc, renderMeshArray, materialMeshInfo);
                }
                else
                {
                    EntityManager.SetComponentData(entity, materialMeshInfo);
                }

                if (!EntityManager.HasComponent<LocalTransform>(entity))
                {
                    EntityManager.AddComponentData(entity, LocalTransform.Identity);
                }

                EntityManager.RemoveComponent<ChunkRenderBindingPending>(entity);
            }

            toBind.Dispose();
        }
    }
}
