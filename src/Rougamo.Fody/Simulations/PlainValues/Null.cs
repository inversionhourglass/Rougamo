using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Simulations.PlainValues
{
    internal class Null(TypeSimulation? type = null) : PlainValueSimulation(null!)
    {
        private static readonly TypeSimulation _Type = GlobalRefs.TrObject.Simulate(null!);

        public override TypeSimulation Type => type ?? _Type;

        public override IList<Instruction> Load() => [Instruction.Create(OpCodes.Ldnull)];

        public override IList<Instruction> Cast(TypeReference to) => [];
    }

    internal static class NullExtensions
    {
        public static Null Null(this TypeSimulation type) => new(type);
    }
}
