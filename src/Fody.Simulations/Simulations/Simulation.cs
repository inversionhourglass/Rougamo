namespace Fody.Simulations
{
    public abstract class Simulation(SimulationModuleWeaver moduleWeaver)
    {
        public SimulationModuleWeaver ModuleWeaver { get; } = moduleWeaver;
    }
}
