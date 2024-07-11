using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations
{
    internal interface ILoadable
    {
        TypeReference TypeRef { get; }

        IList<Instruction> Load();
    }
}
