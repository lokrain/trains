using Unity.Entities;

namespace OpenTTD.Core.Map
{
    /// <summary>
    /// Marks a map entity whose chunk meshes were already built.
    /// </summary>
    public struct MapChunkMeshesBuiltTag : IComponentData
    {
    }

    /// <summary>
    /// Tracks incremental chunk mesh build progress for a map entity.
    /// </summary>
    public struct MapChunkMeshBuildProgress : IComponentData
    {
        public int NextChunkIndex;
    }
}
