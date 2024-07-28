using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations.Operations
{
    internal class BitAnd(ILoadable value1, ILoadable value2) : ILoadable
    {
        public ModuleWeaver ModuleWeaver => value1.ModuleWeaver ?? value2.ModuleWeaver;

        public TypeSimulation Type => value1.Type ?? value2.Type;

        public OpCode TrueToken => value1.TrueToken;

        public OpCode FalseToken => value2.FalseToken;

        public IList<Instruction> Cast(TypeReference to)
        {
            return Type.Cast(to);
        }

        public IList<Instruction> Load()
        {
            return [.. value1.Load(), Instruction.Create(OpCodes.And), .. value2.Load()];
        }
    }
}
