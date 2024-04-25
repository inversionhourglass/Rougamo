using Mono.Cecil.Cil;
using System.Runtime.CompilerServices;

namespace Rougamo.Fody.Enhances.Iterator
{
    internal class StrictIteratorAnchor : IAnchors
    {
        public Instruction StateIs0 { get; } = Nop();

        public Instruction StateIs1 { get; } = Nop();

        public Instruction HasNextBrTo { get; } = Nop();

        public Instruction SetStateTo1 { get; } = Nop();

        public Instruction SetStateToM1 { get; } = Nop();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Instruction Nop() => Instruction.Create(OpCodes.Nop);
    }
}
