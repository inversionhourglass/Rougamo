using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rougamo.Fody.Simulations.PlainValues
{
    [DebuggerDisplay("int({value})")]
    internal class Int32Value(int value, ModuleWeaver moduleWeaver) : PlainValueSimulation(moduleWeaver), IParameterSimulation
    {
        public override TypeSimulation Type => ModuleWeaver._simulations.Int32;

        public override IList<Instruction> Load() => [Instruction.Create(OpCodes.Ldc_I4, value)];
    }
}
