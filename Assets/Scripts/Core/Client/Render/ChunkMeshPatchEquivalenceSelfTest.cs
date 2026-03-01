#nullable enable
using System;
using Unity.Collections;
using UnityEngine;
using OpenTTD.Core.World;

namespace OpenTTD.Core.Client.Render
{
    /// <summary>
    /// Minimal self-test: applying local rect updates and rebuilding mesh equals expected rebuilt mesh hash.
    /// </summary>
    public static class ChunkMeshPatchEquivalenceSelfTest
    {
        public static bool Run()
        {
            WorldChunkArray worldA = WorldChunkArray.Create(96, Allocator.Temp, Allocator.Temp);
            WorldChunkArray worldB = WorldChunkArray.Create(96, Allocator.Temp, Allocator.Temp);

            try
            {
                InitializeChunk(ref worldA, 0, 0);
                InitializeChunk(ref worldB, 0, 0);

                Mesh? meshA = null;
                Mesh? meshB = null;
                try
                {
                    meshA = ChunkMeshBuilder.BuildOrUpdate(meshA!, ref worldA, 0, 0);
                    meshB = ChunkMeshBuilder.BuildOrUpdate(meshB!, ref worldB, 0, 0);

                    ApplyRect(ref worldA, 0, 0, 2, 2, 4, 3, 140);
                    meshA = ChunkMeshBuilder.BuildOrUpdateRect(meshA, ref worldA, 0, 0, 2, 2, 5, 4);

                    ApplyRect(ref worldA, 0, 0, 10, 5, 2, 2, 200);
                    meshA = ChunkMeshBuilder.BuildOrUpdateRect(meshA, ref worldA, 0, 0, 10, 5, 11, 6);

                    ApplyRect(ref worldB, 0, 0, 2, 2, 4, 3, 140);
                    ApplyRect(ref worldB, 0, 0, 10, 5, 2, 2, 200);

                    meshB = ChunkMeshBuilder.BuildOrUpdate(meshB!, ref worldB, 0, 0);

                    return HashMesh(meshA) == HashMesh(meshB);
                }
                finally
                {
                    if (meshA != null)
                    {
                        global::UnityEngine.Object.Destroy(meshA);
                    }

                    if (meshB != null)
                    {
                        global::UnityEngine.Object.Destroy(meshB);
                    }
                }
            }
            finally
            {
                worldA.Dispose();
                worldB.Dispose();
            }
        }

        private static void InitializeChunk(ref WorldChunkArray world, int cx, int cy)
        {
            ChunkSoA chunk = world.GetChunk(cx, cy);
            for (int i = 0; i < chunk.Height.Length; i++)
            {
                chunk.Height[i] = (byte)(32 + (i % 16));
                chunk.RiverMask[i] = 0;
                chunk.Biome[i] = (byte)(i % 3);
            }

            world.SetChunk(cx, cy, chunk);
        }

        private static void ApplyRect(ref WorldChunkArray world, int cx, int cy, int rx, int ry, int rw, int rh, byte value)
        {
            ChunkSoA chunk = world.GetChunk(cx, cy);
            for (int y = 0; y < rh; y++)
            {
                int baseIdx = (ry + y) * WorldConstants.ChunkSize + rx;
                for (int x = 0; x < rw; x++)
                {
                    chunk.Height[baseIdx + x] = value;
                }
            }

            world.SetChunk(cx, cy, chunk);
        }

        private static ulong HashMesh(Mesh mesh)
        {
            ulong h = 14695981039346656037UL;

            Vector3[] verts = mesh.vertices;
            Color32[] colors = mesh.colors32;
            int[] tris = mesh.triangles;

            for (int i = 0; i < verts.Length; i++)
            {
                h = Mix(h, (ulong)verts[i].GetHashCode());
            }

            for (int i = 0; i < colors.Length; i++)
            {
                h = Mix(h, colors[i].r);
                h = Mix(h, colors[i].g);
                h = Mix(h, colors[i].b);
                h = Mix(h, colors[i].a);
            }

            for (int i = 0; i < tris.Length; i++)
            {
                h = Mix(h, (ulong)(uint)tris[i]);
            }

            return h;
        }

        private static ulong Mix(ulong current, ulong value)
        {
            ulong x = current ^ value;
            x ^= x >> 33;
            x *= 0xff51afd7ed558ccdUL;
            x ^= x >> 33;
            x *= 0xc4ceb9fe1a85ec53UL;
            x ^= x >> 33;
            return x;
        }
    }
}
