#nullable enable
using Unity.Collections;
using UnityEngine;
using OpenTTD.Core.World;

namespace OpenTTD.Core.Client.Render
{
    /// <summary>
    /// Minimal seam regression self-test for neighboring chunk mesh border continuity.
    /// </summary>
    public static class ChunkMeshSeamSelfTest
    {
        public static bool Run()
        {
            WorldChunkArray world = WorldChunkArray.Create(96, Allocator.Temp, Allocator.Temp);

            try
            {
                FillChunkFromWorldFormula(ref world, 0, 0);
                FillChunkFromWorldFormula(ref world, 1, 0);

                Mesh? left = null;
                Mesh? right = null;
                try
                {
                    left = ChunkMeshBuilder.BuildOrUpdate(left, ref world, 0, 0);
                    right = ChunkMeshBuilder.BuildOrUpdate(right, ref world, 1, 0);

                    Vector3[] leftVerts = left.vertices;
                    Vector3[] rightVerts = right.vertices;
                    Color32[] leftColors = left.colors32;
                    Color32[] rightColors = right.colors32;

                    const int vertsPerSide = 65;
                    for (int y = 0; y < vertsPerSide; y++)
                    {
                        int leftIndex = 64 + y * vertsPerSide;
                        int rightIndex = y * vertsPerSide;

                        float lh = leftVerts[leftIndex].y;
                        float rh = rightVerts[rightIndex].y;
                        if (Mathf.Abs(lh - rh) > 0.0001f)
                        {
                            return false;
                        }

                        if (!leftColors[leftIndex].Equals(rightColors[rightIndex]))
                        {
                            return false;
                        }
                    }

                    return true;
                }
                finally
                {
                    if (left != null)
                    {
                        global::UnityEngine.Object.Destroy(left);
                    }

                    if (right != null)
                    {
                        global::UnityEngine.Object.Destroy(right);
                    }
                }
            }
            finally
            {
                world.Dispose();
            }
        }

        private static void FillChunkFromWorldFormula(ref WorldChunkArray world, int chunkX, int chunkY)
        {
            ChunkSoA chunk = world.GetChunk(chunkX, chunkY);

            for (int localY = 0; localY < WorldConstants.ChunkSize; localY++)
            {
                for (int localX = 0; localX < WorldConstants.ChunkSize; localX++)
                {
                    int worldX = (chunkX << 6) + localX;
                    int worldY = (chunkY << 6) + localY;
                    int i = WorldConstants.TileIndex(localX, localY);

                    chunk.Height[i] = (byte)((worldX + worldY) & 0xFF);
                    chunk.RiverMask[i] = (byte)(((worldX + worldY) % 19) == 0 ? 1 : 0);
                    chunk.Biome[i] = (byte)((worldX / 16 + worldY / 16) % 3);
                }
            }

            world.SetChunk(chunkX, chunkY, chunk);
        }
    }
}
