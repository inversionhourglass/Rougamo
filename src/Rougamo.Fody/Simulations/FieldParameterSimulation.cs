using Mono.Cecil.Cil;
using Mono.Cecil;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Simulations
{
    internal class FieldParameterSimulation(ILoadable target, FieldReference fieldRef, ModuleDefinition moduleDef) : ParameterSimulation(moduleDef)
    {
        public override Instruction[] Load()
        {
            return [.. target.Load(), Create(OpCodes.Ldfld, fieldRef)];
        }

        public override Instruction[] LoadAddress()
        {
            return [..target.Load(), Create(OpCodes.Ldflda, fieldRef)];
        }
    }
}
