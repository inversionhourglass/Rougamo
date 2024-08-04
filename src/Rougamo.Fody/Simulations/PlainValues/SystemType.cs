using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rougamo.Fody.Simulations.PlainValues
{
    [DebuggerDisplay("{typeRef}")]
    internal class SystemType(TypeReference typeRef, ModuleWeaver moduleWeaver) : PlainValueSimulation(moduleWeaver)
    {
        public override TypeSimulation Type => ModuleWeaver._simulations.Type;

        public override IList<Instruction> Load()
        {
            return [Instruction.Create(OpCodes.Ldtoken, typeRef), Instruction.Create(OpCodes.Call, ModuleWeaver._mGetTypeFromHandleRef)];
        }
    }
}
