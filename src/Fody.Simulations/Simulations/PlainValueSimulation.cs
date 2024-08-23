using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Fody.Simulations
{
    public abstract class PlainValueSimulation(SimulationModuleWeaver moduleWeaver) : Simulation(moduleWeaver), IParameterSimulation
    {
        public OpCode TrueToken => OpCodes.Brtrue;

        public OpCode FalseToken => OpCodes.Brfalse;

        public abstract TypeSimulation Type { get; }

        public abstract IList<Instruction> Load();

        public virtual IList<Instruction> Cast(TypeReference to) => Type.Cast(to);

        public IList<Instruction> LoadAddress(MethodSimulation? method) => throw new FodyWeavingException($"Do not support load {GetType().Name} value by reference.");

        public IList<Instruction> PrepareLoadAddress(MethodSimulation? method) => throw new FodyWeavingException($"Do not support load {GetType().Name} value by reference.");
    }
}
