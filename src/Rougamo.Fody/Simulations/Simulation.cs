using Fody;

namespace Rougamo.Fody.Simulations
{
    internal abstract class Simulation(BaseModuleWeaver moduleWeaver)
    {
        public BaseModuleWeaver ModuleWeaver { get; } = moduleWeaver;
    }
}
