using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations
{
    internal abstract class PlainValueSimulation(ModuleDefinition moduleDef) : Simulation(moduleDef), IParameterSimulation
    {
        public abstract TypeSimulation Type { get; }

        public abstract IList<Instruction> Load();

        public virtual IList<Instruction> Cast(TypeReference to) => Type.Cast(to);

        public IList<Instruction> LoadAddress(MethodSimulation method) => throw new RougamoException($"Do not support load {GetType().Name} value by reference.");

        public IList<Instruction> PrepareLoadAddress(MethodSimulation method) => throw new RougamoException($"Do not support load {GetType().Name} value by reference.");
    }
}
