using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations
{
    internal interface ICallingTarget
    {
        IList<Instruction>? LoadForCallingMethod();
    }
}
