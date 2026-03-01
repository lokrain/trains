#nullable enable
using Unity.Collections;
using UnityEngine;
using OpenTTD.Core.World;

namespace OpenTTD.Core.Client.Render
{
    /// <summary>
    /// Minimal determinism self-test for chunk mesh generation.
    /// </summary>
    public static class ChunkMeshDeterminismSelfTest
    {
        public static bool Run()
        {
            WorldChunkArray world = WorldChunkArray.Create(96, Allocator.Temp, Allocator.Temp);

            try
            {
                ChunkSoA chunk = world.GetChunk(0, 0);
                for (int i = 0; i < chunk.Height.Length; i++)
                {
                    chunk.Height[i] = (byte)(i & 0xFF);
                    chunk.RiverMask[i] = (byte)((i % 37) == 0 ? 1 : 0);
                    chunk.Biome[i] = (byte)(i % 3);
                }

                world.SetChunk(0, 0, chunk);

                Mesh? a = null;
                Mesh? b = null;
                try
                {
                    // Fix: Use null-forgiving operator to indicate intentional null usage for Mesh parameter.
                    a = ChunkMeshBuilder.BuildOrUpdate(a!, ref world, 0, 0);
                    b = ChunkMeshBuilder.BuildOrUpdate(b!, ref world, 0, 0);

                    ulong hashA = HashMesh(a);
                    ulong hashB = HashMesh(b);
                    return hashA == hashB;
                }
                finally
                {
                    if (a != null)
                    {
                        Object.Destroy(a);
                    }

                    if (b != null)
                    {
                        Object.Destroy(b);
                    }
                }
            }
            finally
            {
                world.Dispose();
            }
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
