using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Fody.Simulations
{
    public interface IParameterSimulation : ILoadable
    {
        IList<Instruction> PrepareLoadAddress(MethodSimulation? method);

        IList<Instruction> LoadAddress(MethodSimulation? method);
    }
}
