using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations.Parameters
{
    internal class RawArgument(IList<Instruction> instructions) : UnableLoadAddress(null!)
    {
        public override TypeReference TypeRef => GlobalRefs.TrObject;

        public override IList<Instruction> Load() => instructions;
    }
}
