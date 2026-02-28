#nullable enable
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using OpenTTD.Core.World;

namespace OpenTTD.Core.Client.Render
{
    /// <summary>
    /// Chunk mesh builder (client-side):
    /// - Builds a 65x65 shared-vertex grid for seamless edges.
    /// - Uses height_u8 for Y and simple biome/water tint per vertex.
    /// </summary>
    public static class ChunkMeshBuilder
    {
        /// <summary>
        /// Builds or updates a mesh for a world chunk.
        /// </summary>
        /// <param name="mesh">Existing mesh instance to reuse, or null to create.</param>
        /// <param name="world">World chunk storage.</param>
        /// <param name="chunkX">Chunk X coordinate.</param>
        /// <param name="chunkY">Chunk Y coordinate.</param>
        /// <returns>Updated mesh instance.</returns>
        public static Mesh BuildOrUpdate(
            Mesh mesh,
            ref WorldChunkArray world,
            int chunkX,
            int chunkY)
        {
            if (mesh == null)
            {
                mesh = new Mesh
                {
                    indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
                };
            }

            mesh.Clear();

            const int vertsPerSide = 65;
            const int vertCount = vertsPerSide * vertsPerSide;
            const int quadPerSide = 64;
            const int indexCount = quadPerSide * quadPerSide * 6;

            var verts = new Vector3[vertCount];
            var colors = new Color32[vertCount];
            var indices = new int[indexCount];

            int v = 0;
            for (int y = 0; y < vertsPerSide; y++)
            {
                for (int x = 0; x < vertsPerSide; x++)
                {
                    int wx = (chunkX << 6) + x;
                    int wy = (chunkY << 6) + y;

                    byte h = TileAccessor.GetHeightClamped(ref world, wx, wy);

                    int tx = math.min(wx, WorldConstants.MapW - 1);
                    int ty = math.min(wy, WorldConstants.MapH - 1);
                    TileAccessor.WorldToChunkLocal(tx, ty, out int cx, out int cy, out int lx, out int ly);
                    ChunkSoA c = world.GetChunk(cx, cy);
                    int tidx = WorldConstants.TileIndex(lx, ly);

                    bool isRiver = c.RiverMask[tidx] != 0;
                    bool isSea = !isRiver && c.Height[tidx] <= world.SeaLevel;

                    Color32 col;
                    if (isRiver)
                    {
                        col = new Color32(30, 80, 200, 255);
                    }
                    else if (isSea)
                    {
                        col = new Color32(10, 40, 140, 255);
                    }
                    else
                    {
                        byte biome = c.Biome[tidx];
                        switch (biome)
                        {
                            case 2:
                                col = new Color32(90, 90, 90, 255);
                                break;
                            case 1:
                                col = new Color32(70, 120, 70, 255);
                                break;
                            default:
                                col = new Color32(60, 150, 60, 255);
                                break;
                        }
                    }

                    verts[v] = new Vector3(x, h * 0.25f, y);
                    colors[v] = col;
                    v++;
                }
            }

            int ii = 0;
            for (int y = 0; y < quadPerSide; y++)
            {
                for (int x = 0; x < quadPerSide; x++)
                {
                    int i0 = x + y * vertsPerSide;
                    int i1 = i0 + 1;
                    int i2 = i0 + vertsPerSide;
                    int i3 = i2 + 1;

                    indices[ii++] = i0;
                    indices[ii++] = i2;
                    indices[ii++] = i1;

                    indices[ii++] = i1;
                    indices[ii++] = i2;
                    indices[ii++] = i3;
                }
            }

            mesh.vertices = verts;
            mesh.colors32 = colors;
            mesh.triangles = indices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
