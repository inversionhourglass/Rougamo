using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations
{
    internal interface IParameterSimulation : ILoadable
    {
        IList<Instruction>? PrepareLoadAddress(MethodSimulation method);

        IList<Instruction> LoadAddress(MethodSimulation method);
    }
}
