using Mono.Cecil.Cil;

namespace Rougamo.Fody.Enhances.Sync
{
    internal class CtorContext : SyncContext
    {
        public Instruction LeaveToCatchEnd { get; } = Instruction.Create(OpCodes.Nop);
    }
}
