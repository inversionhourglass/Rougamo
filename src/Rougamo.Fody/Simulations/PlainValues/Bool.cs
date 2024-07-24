using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rougamo.Fody.Simulations.PlainValues
{
    [DebuggerDisplay("bool({value})")]
    internal sealed class Bool(bool value, ModuleWeaver moduleWeaver) : PlainValueSimulation(moduleWeaver)
    {
        public override TypeSimulation Type => ModuleWeaver._simulations.Bool;

        public override IList<Instruction> Load() => [value ? Instruction.Create(OpCodes.Ldc_I4_1) : Instruction.Create(OpCodes.Ldc_I4_0)];
    }
}
