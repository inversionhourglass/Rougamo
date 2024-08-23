using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Fody.Simulations
{
    public interface IHost : IParameterSimulation
    {
        IList<Instruction> LoadAny();
    }
}
