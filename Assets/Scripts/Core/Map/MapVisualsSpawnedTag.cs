using Unity.Entities;

namespace OpenTTD.Core.Map
{
    /// <summary>
    /// Marks a map entity whose visual tiles were already instantiated.
    /// </summary>
    public struct MapVisualsSpawnedTag : IComponentData
    {
    }
}
