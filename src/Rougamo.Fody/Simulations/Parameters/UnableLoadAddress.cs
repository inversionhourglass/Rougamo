using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations.Parameters
{
    internal abstract class UnableLoadAddress(ModuleDefinition moduleDef) : Simulation(moduleDef), IParameterSimulation
    {
        public abstract TypeReference TypeRef { get; }

        public abstract IList<Instruction> Load();

        public IList<Instruction> LoadAddress(MethodSimulation method) => throw new RougamoException($"Do not support load {GetType().Name} value by reference.");

        public IList<Instruction> PrepareLoadAddress(MethodSimulation method) => throw new RougamoException($"Do not support load {GetType().Name} value by reference.");
    }
}
