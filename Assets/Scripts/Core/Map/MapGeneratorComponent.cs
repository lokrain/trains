using Unity.Entities;
using Unity.Entities;
using Unity.Mathematics;

namespace OpenTTD.Core.Map
{
    public struct MapGeneratorComponent : IComponentData
    {
        public int2 MapSize;
        public int Seed;
        public float NoiseScale;
    }
}