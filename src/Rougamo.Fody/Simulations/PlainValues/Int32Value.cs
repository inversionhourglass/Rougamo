using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations.PlainValues
{
    internal class Int32Value(int value) : PlainValueSimulation(null!), IParameterSimulation
    {
        public override TypeSimulation Type => GlobalSimulations.Int32;

        public override IList<Instruction> Load() => [Instruction.Create(OpCodes.Ldc_I4, value)];
    }
}
