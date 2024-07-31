using Mono.Cecil.Cil;

namespace Rougamo.Fody.Enhances.Iterator
{
    internal class IteratorContext
    {
        public Instruction AnchorState1 { get; } = Instruction.Create(OpCodes.Nop);
    }
}
