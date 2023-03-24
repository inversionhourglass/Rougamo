using Mono.Cecil.Cil;
using System.Runtime.CompilerServices;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Enhances
{
    internal class SyncAnchors
    {
        public SyncAnchors(SyncVariables variables, Instruction tryStart)
        {
            TryStart = tryStart;
            CatchStart = Create(OpCodes.Stloc, variables.Exception);
        }

        public Instruction InitContextStart { get; } = Nop();

        public Instruction OnEntryStart { get; } = Nop();

        public Instruction IfOnEntryReplacedStart { get; } = Nop();

        public Instruction RewriteArgStart { get; } = Nop();

        public Instruction RetryStart { get; } = Nop();

        public Instruction TryStart { get; }

        public Instruction CatchStart { get; }

        public Instruction OnExceptionStart { get; } = Nop();

        public Instruction IfOnExceptionRetryStart { get; } = Nop();

        public Instruction IfOnExceptionHandledStart { get; } = Nop();

        public Instruction Rethrow { get; } = Create(OpCodes.Rethrow);

        public Instruction FinallyStart { get; } = Nop();

        public Instruction SetBackReturnStart { get; } = Nop();

        public Instruction OnSuccessStart { get; }= Nop();

        public Instruction IfOnSuccessRetryStart { get; } = Nop();

        public Instruction IfOnSuccessReplacedStart { get; } = Nop();

        public Instruction OnExitStart { get; } = Nop();

        public Instruction EndFinally { get; } = Create(OpCodes.Endfinally);

        public Instruction FinallyEnd { get; set; } = Nop();

        public Instruction ReturnStart { get; set; } = Create(OpCodes.Ret);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Instruction Nop() => Create(OpCodes.Nop);
    }
}
