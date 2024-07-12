using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations.Parameters
{
    internal class SystemMethodBase(MethodDefinition methodDef) : UnableLoadAddress(null!)
    {
        public override TypeReference TypeRef => GlobalRefs.TrObject;

        public override IList<Instruction> Load()
        {
            return [
                Instruction.Create(OpCodes.Ldtoken, methodDef),
                Instruction.Create(OpCodes.Ldtoken, methodDef.DeclaringType),
                Instruction.Create(OpCodes.Call, GlobalRefs.MrGetMethodFromHandle)
            ];
        }
    }
}
