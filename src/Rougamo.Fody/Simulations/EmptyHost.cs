using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations
{
    internal class EmptyHost(ModuleWeaver moduleWeaver) : IHost
    {
        public TypeSimulation TypeRef => moduleWeaver._simulations.Object;

        public TypeSimulation Type => moduleWeaver._simulations.Object;

        public OpCode TrueToken => OpCodes.Brtrue;

        public OpCode FalseToken => OpCodes.Brfalse;

        public ModuleWeaver ModuleWeaver => moduleWeaver;

        public IList<Instruction> Cast(TypeReference to) => [];

        public IList<Instruction> Load() => [];

        public IList<Instruction> LoadAddress(MethodSimulation? method) => [];

        public IList<Instruction> LoadForCallingMethod() => [];

        public IList<Instruction> PrepareLoadAddress(MethodSimulation? method) => [];
    }
}
