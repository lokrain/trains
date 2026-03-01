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
        private const int VertsPerSide = 65;
        private const int QuadPerSide = 64;
        private const int VertCount = VertsPerSide * VertsPerSide;
        private const int IndexCount = QuadPerSide * QuadPerSide * 6;

        /// <summary>
        /// Builds or updates a mesh for a world chunk.
        /// </summary>
        /// <param name="mesh">Existing mesh instance to reuse, or null to create.</param>
        /// <param name="world">World chunk storage.</param>
        /// <param name="chunkX">Chunk X coordinate.</param>
        /// <param name="chunkY">Chunk Y coordinate.</param>
        /// <returns>Updated mesh instance.</returns>
        public static Mesh BuildOrUpdate(
            Mesh? mesh,
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

            var verts = new Vector3[VertCount];
            var colors = new Color32[VertCount];
            var indices = new int[IndexCount];

            int v = 0;
            for (int y = 0; y < VertsPerSide; y++)
            {
                for (int x = 0; x < VertsPerSide; x++)
                {
                    byte height = SampleHeight(ref world, chunkX, chunkY, x, y);
                    Color32 color = SampleColor(ref world, chunkX, chunkY, x, y);

                    verts[v] = new Vector3(x, height * 0.25f, y);
                    colors[v] = color;
                    v++;
                }
            }

            int ii = 0;
            for (int y = 0; y < QuadPerSide; y++)
            {
                for (int x = 0; x < QuadPerSide; x++)
                {
                    int i0 = x + y * VertsPerSide;
                    int i1 = i0 + 1;
                    int i2 = i0 + VertsPerSide;
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

        /// <summary>
        /// Partially updates mesh vertices/colors for a dirty tile rect and reuses existing topology.
        /// Falls back to full rebuild when mesh layout is not initialized.
        /// </summary>
        public static Mesh BuildOrUpdateRect(
            Mesh? mesh,
            ref WorldChunkArray world,
            int chunkX,
            int chunkY,
            byte minX,
            byte minY,
            byte maxX,
            byte maxY)
        {
            if (mesh == null)
            {
                return BuildOrUpdate(null, ref world, chunkX, chunkY);
            }

            Vector3[] verts = mesh.vertices;
            Color32[] colors = mesh.colors32;
            if (verts == null || colors == null || verts.Length != VertCount || colors.Length != VertCount)
            {
                return BuildOrUpdate(mesh, ref world, chunkX, chunkY);
            }

            int vxMin = math.max(0, minX);
            int vyMin = math.max(0, minY);
            int vxMax = math.min(64, maxX + 1);
            int vyMax = math.min(64, maxY + 1);

            for (int y = vyMin; y <= vyMax; y++)
            {
                for (int x = vxMin; x <= vxMax; x++)
                {
                    byte height = SampleHeight(ref world, chunkX, chunkY, x, y);
                    Color32 color = SampleColor(ref world, chunkX, chunkY, x, y);

                    int vertexIndex = x + y * VertsPerSide;
                    verts[vertexIndex] = new Vector3(x, height * 0.25f, y);
                    colors[vertexIndex] = color;
                }
            }

            mesh.vertices = verts;
            mesh.colors32 = colors;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static byte SampleHeight(ref WorldChunkArray world, int chunkX, int chunkY, int localX, int localY)
        {
            int worldX = (chunkX << 6) + localX;
            int worldY = (chunkY << 6) + localY;
            return TileAccessor.GetHeightClamped(ref world, worldX, worldY);
        }

        private static Color32 SampleColor(ref WorldChunkArray world, int chunkX, int chunkY, int localX, int localY)
        {
            int worldX = (chunkX << 6) + localX;
            int worldY = (chunkY << 6) + localY;

            int clampedWorldX = math.min(worldX, WorldConstants.MapW - 1);
            int clampedWorldY = math.min(worldY, WorldConstants.MapH - 1);
            TileAccessor.WorldToChunkLocal(clampedWorldX, clampedWorldY, out int sampleChunkX, out int sampleChunkY, out int sampleLocalX, out int sampleLocalY);

            ChunkSoA chunk = world.GetChunk(sampleChunkX, sampleChunkY);
            int tileIndex = WorldConstants.TileIndex(sampleLocalX, sampleLocalY);

            bool isRiver = chunk.RiverMask[tileIndex] != 0;
            if (isRiver)
            {
                return new Color32(30, 80, 200, 255);
            }

            bool isSea = chunk.Height[tileIndex] <= world.SeaLevel;
            if (isSea)
            {
                return new Color32(10, 40, 140, 255);
            }

            return BiomeToColor(chunk.Biome[tileIndex]);
        }

        private static Color32 BiomeToColor(byte biome)
        {
            return biome switch
            {
                2 => new Color32(90, 90, 90, 255),
                1 => new Color32(70, 120, 70, 255),
                _ => new Color32(60, 150, 60, 255),
            };
        }
    }
}
