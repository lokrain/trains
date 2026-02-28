using Unity.Collections;
using Unity.Entities;

namespace OpenTTD.Core.Map
{
    public struct MapDataComponent : IComponentData
    {
        public NativeArray<TileData> Map;
    }
}