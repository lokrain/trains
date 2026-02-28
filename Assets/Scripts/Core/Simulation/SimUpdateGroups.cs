#nullable enable
using Unity.Entities;

namespace OpenTTD.Core.Simulation
{
    /// <summary>
    /// Deterministic simulation initialization phase in fixed-step loop.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderFirst = true)]
    public partial class SimInitGroup : ComponentSystemGroup
    {
    }

    /// <summary>
    /// Authoritative simulation tick phase.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(SimInitGroup))]
    public partial class SimTickGroup : ComponentSystemGroup
    {
    }

    /// <summary>
    /// Post-simulation deterministic phase.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(SimTickGroup))]
    public partial class PostSimGroup : ComponentSystemGroup
    {
    }

    /// <summary>
    /// Outbound network send phase after simulation updates.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(PostSimGroup))]
    public partial class NetSendGroup : ComponentSystemGroup
    {
    }
}
