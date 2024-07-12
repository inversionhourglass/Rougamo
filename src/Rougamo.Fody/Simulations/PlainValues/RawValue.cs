using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations.PlainValues
{
    internal class RawValue(TypeSimulation type, IList<Instruction> instructions) : PlainValueSimulation(null!)
    {
        public override TypeSimulation Type => type;

        public override IList<Instruction> Load() => instructions;
    }
}
