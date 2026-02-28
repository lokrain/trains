using Unity.Entities;
using OpenTTD.Core.World;

namespace OpenTTD.Core.Map
{
    public struct MapDataComponent : IComponentData
    {
        public WorldChunkArray World;
    }
}