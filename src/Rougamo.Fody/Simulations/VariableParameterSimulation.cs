using Mono.Cecil;
using Mono.Cecil.Cil;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Simulations
{
    internal class VariableParameterSimulation(VariableDefinition variable, ModuleDefinition moduleDef) : ParameterSimulation(moduleDef)
    {
        public override Instruction[] Load()
        {
            return [Create(OpCodes.Ldloc, variable)];
        }

        public override Instruction[] LoadAddress()
        {
            return [Create(OpCodes.Ldloca, variable)];
        }

        public static implicit operator VariableParameterSimulation(VariableDefinition variable) => new(variable, variable.VariableType.Module);
    }
}
