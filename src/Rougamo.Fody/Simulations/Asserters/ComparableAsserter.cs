using Mono.Cecil.Cil;
using Mono.Cecil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations.Asserters
{
    internal abstract class ComparableAsserter(ILoadable value1, ILoadable value2) : ILoadable
    {
        public TypeSimulation Type => GlobalSimulations.Bool;

        public abstract OpCode TrueToken { get; }

        public abstract OpCode FalseToken { get; }

        public IList<Instruction> Cast(TypeReference to) => Type.Cast(to);

        public IList<Instruction> Load()
        {
            return [.. value1.Load(), .. value2.Load()];
        }
    }
}
