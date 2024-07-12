using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations.Parameters
{
    internal class Bool(bool value) : UnableLoadAddress(null!)
    {
        public override TypeReference TypeRef => GlobalRefs.TrBool;

        public override IList<Instruction> Load() => [value ? Instruction.Create(OpCodes.Ldc_I4_1) : Instruction.Create(OpCodes.Ldc_I4_0)];
    }
}
