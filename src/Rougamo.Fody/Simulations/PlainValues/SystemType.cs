using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations.PlainValues
{
    internal class SystemType(TypeReference typeRef) : PlainValueSimulation(null!)
    {
        public override TypeSimulation Type => GlobalSimulations.Type;

        public override IList<Instruction> Load()
        {
            return [Instruction.Create(OpCodes.Ldtoken, typeRef), Instruction.Create(OpCodes.Call, GlobalRefs.MrGetTypeFromHandle)];
        }
    }
}
