#nullable enable
using Unity.Entities;

namespace OpenTTD.Presentation.WorldRendering
{
    /// <summary>
    /// Tag identifying one chunk presenter entity.
    /// </summary>
    public struct ChunkRenderTag : IComponentData
    {
    }

    /// <summary>
    /// Chunk coordinate mapped to a presenter entity.
    /// </summary>
    public struct ChunkCoordComponent : IComponentData
    {
        public short X;
        public short Y;
    }

    /// <summary>
    /// Last world snapshot version committed to the presenter mesh.
    /// </summary>
    public struct ChunkRenderVersion : IComponentData
    {
        public uint Value;
    }

    /// <summary>
    /// Latest world snapshot version queued for mesh rebuild/commit.
    /// </summary>
    public struct ChunkRenderPendingVersion : IComponentData
    {
        public uint Value;
    }

    /// <summary>
    /// Dirty mode for pending mesh rebuilds.
    /// </summary>
    public enum ChunkMeshDirtyMode : byte
    {
        None = 0,
        Full = 1,
        Rect = 2
    }

    /// <summary>
    /// Chunk mesh dirty state and optional rect bounds.
    /// </summary>
    public struct ChunkMeshDirty : IComponentData
    {
        public ChunkMeshDirtyMode Mode;
        public byte MinX;
        public byte MinY;
        public byte MaxX;
        public byte MaxY;
    }

    /// <summary>
    /// Material variant selector for chunk visual modes.
    /// </summary>
    public struct ChunkMaterialVariant : IComponentData
    {
        public byte Value;
    }

    /// <summary>
    /// Marker indicating presenter needs Entities Graphics binding setup.
    /// </summary>
    public struct ChunkRenderBindingPending : IComponentData
    {
    }

    /// <summary>
    /// Singleton render resource selector for presenter mesh/material indices.
    /// Attach <see cref="Unity.Rendering.RenderMeshArray"/> shared component on the same entity.
    /// </summary>
    public struct ChunkPresenterRenderResources : IComponentData
    {
        public short MaterialIndex;
        public short MeshIndex;
        public byte CastShadows;
        public byte ReceiveShadows;
    }

    /// <summary>
    /// Singleton marker for AOI-driven presenter lifecycle input.
    /// Attach <see cref="VisibleChunkElement"/> buffer on same entity.
    /// </summary>
    public struct ChunkPresenterAoiSource : IComponentData
    {
    }

    /// <summary>
    /// Dynamic buffer element describing currently visible chunks.
    /// </summary>
    public struct VisibleChunkElement : IBufferElementData
    {
        public short X;
        public short Y;
    }

    /// <summary>
    /// Singleton marker for render invalidation events emitted by snapshot/patch world apply paths.
    /// Attach <see cref="ChunkRenderInvalidationEvent"/> buffer on same entity.
    /// </summary>
    public struct ChunkRenderInvalidationSource : IComponentData
    {
    }

    /// <summary>
    /// Invalidation event describing a full or rect rebuild for one chunk at a specific snapshot version.
    /// </summary>
    public struct ChunkRenderInvalidationEvent : IBufferElementData
    {
        public short ChunkX;
        public short ChunkY;
        public uint SnapshotVersion;
        public ChunkMeshDirtyMode Mode;
        public byte MinX;
        public byte MinY;
        public byte MaxX;
        public byte MaxY;
    }
}
