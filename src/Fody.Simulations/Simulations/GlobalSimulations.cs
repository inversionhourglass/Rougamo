namespace Fody.Simulations
{
    public class GlobalSimulations(SimulationModuleWeaver moduleWeaver)
    {
        public TypeSimulation Object { get; } = moduleWeaver._tObjectRef.Simulate(moduleWeaver);
        public TypeSimulation Bool { get; } = moduleWeaver._tBooleanRef.Simulate(moduleWeaver);
        public TypeSimulation Int32 { get; } = moduleWeaver._tInt32Ref.Simulate(moduleWeaver);
        public TypeSimulation Type { get; } = moduleWeaver._tTypeRef.Simulate(moduleWeaver);
        public TypeSimulation MethodBase { get; } = moduleWeaver._tMethodBaseRef.Simulate(moduleWeaver);
    }
}
