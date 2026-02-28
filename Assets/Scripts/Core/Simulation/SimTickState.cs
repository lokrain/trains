#nullable enable
using Unity.Entities;

namespace OpenTTD.Core.Simulation
{
    /// <summary>
    /// Authoritative fixed-step simulation clock state.
    /// </summary>
    public struct SimTickState : IComponentData
    {
        public ulong Tick;
        public float FixedDeltaTime;
        public int MaxCatchUpSteps;
    }
}
