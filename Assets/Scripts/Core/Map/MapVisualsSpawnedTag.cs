using Unity.Entities;

namespace OpenTTD.Core.Map
{
    /// <summary>
    /// Marks a map entity whose chunk meshes were already built.
    /// </summary>
    public struct MapChunkMeshesBuiltTag : IComponentData
    {
    }
}
