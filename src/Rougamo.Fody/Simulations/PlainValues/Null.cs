using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations.PlainValues
{
    internal class Null(ModuleWeaver moduleWeaver, TypeSimulation? type = null) : PlainValueSimulation(moduleWeaver)
    {
        public override TypeSimulation Type => type ?? ModuleWeaver._simulations.Object;

        public override IList<Instruction> Load() => [Instruction.Create(OpCodes.Ldnull)];

        public override IList<Instruction> Cast(TypeReference to) => [];
    }

    internal static class NullExtensions
    {
        public static Null Null(this TypeSimulation type) => new(type.ModuleWeaver, type);
    }
}
