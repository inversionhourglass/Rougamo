using Mono.Cecil.Cil;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Contexts
{
    internal class SyncContext
    {
        public Instruction AnchorRetry { get; } = Create(OpCodes.Nop);

        public Instruction AnchorReturnResult { get; } = Create(OpCodes.Nop);
    }
}
