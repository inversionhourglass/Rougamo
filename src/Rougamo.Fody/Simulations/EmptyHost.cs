using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rougamo.Fody.Simulations
{
    [DebuggerDisplay("")]
    internal class EmptyHost : IHost
    {
        public TypeSimulation TypeRef => GlobalSimulations.Object;

        public TypeSimulation Type => GlobalSimulations.Object;

        public OpCode TrueToken => OpCodes.Brtrue;

        public OpCode FalseToken => OpCodes.Brfalse;

        public IList<Instruction> Cast(TypeReference to) => [];

        public IList<Instruction> Load() => [];

        public IList<Instruction> LoadAddress(MethodSimulation method) => [];

        public IList<Instruction> LoadForCallingMethod() => [];

        public IList<Instruction> PrepareLoadAddress(MethodSimulation method) => [];
    }
}
