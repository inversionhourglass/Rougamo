using Mono.Cecil.Cil;
using Mono.Cecil;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Simulations
{
    internal class ThisParameterSimulation(MethodReference methodRef, ModuleDefinition moduleDef) : ParameterSimulation(moduleDef)
    {
        public VariableDefinition? This { get; private set; }

        public override Instruction[] Load()
        {
            return [Create(OpCodes.Ldarg_0)];
        }

        public override Instruction[] LoadAddress()
        {
            This = methodRef.Resolve().Body.CreateVariable(methodRef.DeclaringType);

            return [Create(OpCodes.Ldloca, This)];
        }
    }
}
