namespace Rougamo.Fody.Simulations
{
    internal class GlobalSimulations(ModuleWeaver moduleWeaver)
    {
        public TypeSimulation Object { get; } = moduleWeaver._typeObjectRef.Simulate(moduleWeaver);
        public TypeSimulation Bool { get; } = moduleWeaver._typeBoolRef.Simulate(moduleWeaver);
        public TypeSimulation Int32 { get; } = moduleWeaver._typeIntRef.Simulate(moduleWeaver);
        public TypeSimulation Type { get; } = moduleWeaver._typeSystemRef.Simulate(moduleWeaver);
        public TypeSimulation MethodBase { get; } = moduleWeaver._typeMethodBaseRef.Simulate(moduleWeaver);
    }
}
