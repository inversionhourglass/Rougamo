using Mono.Cecil;

namespace Rougamo.Fody.Simulations
{
    internal abstract class Simulation(ModuleDefinition moduleDef)
    {
        public ModuleDefinition Module { get; } = moduleDef;
    }
}
