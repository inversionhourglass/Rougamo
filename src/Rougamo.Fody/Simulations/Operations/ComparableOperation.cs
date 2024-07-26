using Mono.Cecil.Cil;
using Mono.Cecil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations.Operations
{
    internal abstract class ComparableOperation(ILoadable value1, ILoadable value2) : ILoadable
    {
        public TypeSimulation Type => ModuleWeaver._simulations.Bool;

        public abstract OpCode TrueToken { get; }

        public abstract OpCode FalseToken { get; }

        public ModuleWeaver ModuleWeaver => value1.ModuleWeaver ?? value2.ModuleWeaver;

        public IList<Instruction> Cast(TypeReference to) => Type.Cast(to);

        public IList<Instruction> Load()
        {
            return [.. value1.Load(), .. value2.Load()];
        }
    }
}
