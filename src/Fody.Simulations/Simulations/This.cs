using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Diagnostics;
using static Mono.Cecil.Cil.Instruction;

namespace Fody.Simulations
{
    [DebuggerDisplay("This={Type}")]
    public class This(TypeSimulation type) : IHost
    {
        public TypeSimulation Type => type;

        public OpCode TrueToken => OpCodes.Brtrue;

        public OpCode FalseToken => OpCodes.Brfalse;

        public SimulationModuleWeaver ModuleWeaver => type.ModuleWeaver;

        public IList<Instruction> LoadAny() => [Create(OpCodes.Ldarg_0)];

        public IList<Instruction> PrepareLoadAddress(MethodSimulation? method)
        {
            if (type.IsValueType) return [];

            if (method == null) throw new FodyWeavingException("Must pass the parameter named 'method' when loading 'this' as a reference argument.");
            method.TempThis ??= method.Def.Body.CreateVariable(type);
            return [Create(OpCodes.Ldarg_0), Create(OpCodes.Stloc, method.TempThis)];
        }

        public IList<Instruction> LoadAddress(MethodSimulation? method)
        {
            if (type.IsValueType) return [Create(OpCodes.Ldarg_0)];

            if (method == null) throw new FodyWeavingException("Must pass the parameter named 'method' when loading 'this' as a reference argument.");
            return [Create(OpCodes.Ldloca, method.TempThis)];
        }

        public IList<Instruction> Load() => [Create(OpCodes.Ldarg_0)];

        public IList<Instruction> Cast(TypeReference to) => Type.Cast(to);
    }
}
