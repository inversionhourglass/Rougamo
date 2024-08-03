using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations
{
    internal interface IHost : IParameterSimulation
    {
        IList<Instruction> LoadAny();
    }
}
