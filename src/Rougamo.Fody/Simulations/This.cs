using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Simulations
{
    internal class This(TypeSimulation type) : IHost
    {
        public TypeSimulation TypeRef => type;

        public TypeSimulation Type => throw new System.NotImplementedException();

        public IList<Instruction> LoadForCallingMethod() => [Create(OpCodes.Ldarg_0)];

        public IList<Instruction> PrepareLoadAddress(MethodSimulation method)
        {
            if (type.IsValueType) return [];

            method.TempThis ??= method.Def.Body.CreateVariable(type);
            return [Create(OpCodes.Ldarg_0), Create(OpCodes.Stloc, method.TempThis)];
        }

        public IList<Instruction> LoadAddress(MethodSimulation method)
        {
            return type.IsValueType ? [Instruction.Create(OpCodes.Ldarg_0)] : [Instruction.Create(OpCodes.Ldloca, method.TempThis)];
        }

        public IList<Instruction> Load() => [Create(OpCodes.Ldarg_0)];

        public IList<Instruction> Cast(TypeReference to) => Type.Cast(to);
    }
}
