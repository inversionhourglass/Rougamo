using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Rougamo.Fody.Simulations
{
    internal abstract class ParameterSimulation(ModuleDefinition moduleDef) : Simulation(moduleDef)
    {
        public abstract Instruction[] Load();

        public abstract Instruction[] LoadAddress();
    }
}
