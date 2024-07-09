using Mono.Cecil;
using Mono.Cecil.Cil;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Simulations
{
    internal class This(TypeReference typeRef) : IHost
    {
        public Instruction[]? LoadForCallingMethod() => [Create(OpCodes.Ldarg_0)];

        public Instruction[]? PrepareLoad(MethodSimulation method) => null;

        public Instruction[] Load(MethodSimulation method) => [Create(OpCodes.Ldarg_0)];

        public Instruction[]? PrepareLoadAddress(MethodSimulation method)
        {
            if (typeRef.IsValueType) return null;

            method.TempThis ??= method.Def.Body.CreateVariable(typeRef);
            return [Create(OpCodes.Ldarg_0), Create(OpCodes.Stloc, method.TempThis)];
        }

        public Instruction[] LoadAddress(MethodSimulation method)
        {
            return typeRef.IsValueType ? [Instruction.Create(OpCodes.Ldarg_0)] : [Instruction.Create(OpCodes.Ldloca, method.TempThis)];
        }
    }
}
