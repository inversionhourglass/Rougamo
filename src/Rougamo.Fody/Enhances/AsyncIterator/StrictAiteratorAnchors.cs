using Mono.Cecil.Cil;
using System.Runtime.CompilerServices;

namespace Rougamo.Fody.Enhances.AsyncIterator
{
    internal class StrictAiteratorAnchors : IAnchors
    {
        public Instruction ReadyProxyCall { get; } = Nop();

        public Instruction CheckStateIs0 { get; } = Nop();

        public Instruction CheckHasNext { get; } = Nop();

        public Instruction SetStateToM4 { get; } = Nop();

        public Instruction SetDisposedToTrue { get; } = Nop();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Instruction Nop() => Instruction.Create(OpCodes.Nop);
    }
}
