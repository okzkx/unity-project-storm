using Unity.Entities;

namespace Modules.FlowField {
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class FlowFieldGroup : ComponentSystemGroup {}
}