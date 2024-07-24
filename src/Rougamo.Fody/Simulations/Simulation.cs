namespace Rougamo.Fody.Simulations
{
    internal abstract class Simulation(ModuleWeaver moduleWeaver)
    {
        public ModuleWeaver ModuleWeaver { get; } = moduleWeaver;
    }
}
