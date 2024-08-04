using Mono.Cecil.Cil;

namespace Rougamo.Fody.Contexts
{
    internal class IteratorContext
    {
        public Instruction AnchorState1 { get; } = Instruction.Create(OpCodes.Nop);
    }
}
