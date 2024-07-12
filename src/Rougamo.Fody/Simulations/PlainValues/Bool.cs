using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations.PlainValues
{
    internal sealed class Bool(bool value) : PlainValueSimulation(null!)
    {
        public override TypeSimulation Type => GlobalSimulations.Bool;

        public override IList<Instruction> Load() => [value ? Instruction.Create(OpCodes.Ldc_I4_1) : Instruction.Create(OpCodes.Ldc_I4_0)];
    }
}
