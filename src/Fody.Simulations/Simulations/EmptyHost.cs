using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Fody.Simulations
{
    public class EmptyHost(SimulationModuleWeaver moduleWeaver) : IHost
    {
        public TypeSimulation TypeRef => moduleWeaver._simulations.Object;

        public TypeSimulation Type => moduleWeaver._simulations.Object;

        public OpCode TrueToken => OpCodes.Brtrue;

        public OpCode FalseToken => OpCodes.Brfalse;

        public SimulationModuleWeaver ModuleWeaver => moduleWeaver;

        public IList<Instruction> Cast(TypeReference to) => [];

        public IList<Instruction> Load() => [];

        public IList<Instruction> LoadAddress(MethodSimulation? method) => [];

        public IList<Instruction> LoadAny() => [];

        public IList<Instruction> PrepareLoadAddress(MethodSimulation? method) => [];
    }
}
