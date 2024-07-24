using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rougamo.Fody.Simulations.PlainValues
{
    [DebuggerDisplay("RawValue, Type = {Type}")]
    internal class RawValue(TypeSimulation type, IList<Instruction> instructions) : PlainValueSimulation(type.ModuleWeaver)
    {
        public override TypeSimulation Type => type;

        public override IList<Instruction> Load() => instructions;
    }
}
