using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations.Parameters
{
    internal class SystemType(TypeReference typeRef) : UnableLoadAddress(null!)
    {
        public override TypeReference TypeRef => CommonRefs.TrType;

        public override IList<Instruction> Load()
        {
            return [Instruction.Create(OpCodes.Ldtoken, typeRef), Instruction.Create(OpCodes.Call, CommonRefs.MrGetTypeFromHandle)];
        }
    }
}
