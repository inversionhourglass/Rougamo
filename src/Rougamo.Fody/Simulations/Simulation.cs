using Mono.Cecil;

namespace Rougamo.Fody.Simulations
{
    internal abstract class Simulation
    {
        protected Simulation(ModuleDefinition moduleDef)
        {
            Module = moduleDef;
        }

        public ModuleDefinition Module { get; }
    }
}
