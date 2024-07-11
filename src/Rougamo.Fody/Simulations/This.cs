using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Simulations
{
    internal class This(TypeReference typeRef) : IHost
    {
        public TypeReference TypeRef => typeRef;

        public IList<Instruction> LoadForCallingMethod() => [Create(OpCodes.Ldarg_0)];

        public IList<Instruction> PrepareLoadAddress(MethodSimulation method)
        {
            if (typeRef.IsValueType) return [];

            method.TempThis ??= method.Def.Body.CreateVariable(typeRef);
            return [Create(OpCodes.Ldarg_0), Create(OpCodes.Stloc, method.TempThis)];
        }

        public IList<Instruction> LoadAddress(MethodSimulation method)
        {
            return typeRef.IsValueType ? [Instruction.Create(OpCodes.Ldarg_0)] : [Instruction.Create(OpCodes.Ldloca, method.TempThis)];
        }

        public IList<Instruction> Load() => [Create(OpCodes.Ldarg_0)];
    }
}
