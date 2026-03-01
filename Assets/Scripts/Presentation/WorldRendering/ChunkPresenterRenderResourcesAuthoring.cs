#nullable enable
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace OpenTTD.Presentation.WorldRendering
{
    /// <summary>
    /// Authoring component that provides render resources for chunk presenters.
    /// Creates a singleton entity with <see cref="ChunkPresenterRenderResources"/> and <see cref="RenderMeshArray"/>.
    /// </summary>
    public sealed class ChunkPresenterRenderResourcesAuthoring : MonoBehaviour
    {
        [SerializeField] private Material[] _materials = new Material[0];
        [SerializeField] private Mesh[] _meshes = new Mesh[0];
        [SerializeField] private short _materialIndex;
        [SerializeField] private short _meshIndex;
        [SerializeField] private bool _castShadows = true;
        [SerializeField] private bool _receiveShadows = true;

        private sealed class ChunkPresenterRenderResourcesBaker : Baker<ChunkPresenterRenderResourcesAuthoring>
        {
            public override void Bake(ChunkPresenterRenderResourcesAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ChunkPresenterRenderResources
                {
                    MaterialIndex = authoring._materialIndex,
                    MeshIndex = authoring._meshIndex,
                    CastShadows = authoring._castShadows ? (byte)1 : (byte)0,
                    ReceiveShadows = authoring._receiveShadows ? (byte)1 : (byte)0
                });

                AddSharedComponentManaged(entity, new RenderMeshArray(authoring._materials, authoring._meshes));
            }
        }
    }
}
