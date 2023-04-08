using Mono.Cecil.Cil;
using System.Runtime.CompilerServices;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Enhances.Sync
{
    internal class SyncAnchors : ITryCatchFinallyAnchors, IAnchors
    {
        public SyncAnchors(SyncVariables variables, Instruction hostsStart)
        {
            HostsStart = hostsStart;
            CatchStart = Create(OpCodes.Stloc, variables.Exception);
        }

        public Instruction InitMos { get; } = Nop();

        public Instruction InitContext { get; } = Nop();

        public Instruction OnEntry { get; } = Nop();

        public Instruction IfEntryReplaced { get; } = Nop();

        public Instruction RewriteArg { get; } = Nop();

        //public Instruction Retry { get; } = Nop();

        public Instruction TryStart { get; } = Nop();

        public Instruction HostsStart { get; }

        public Instruction CatchStart { get; }

        public Instruction OnException { get; } = Nop();

        public Instruction IfExceptionRetry { get; } = Nop();

        public Instruction IfExceptionHandled { get; } = Nop();

        public Instruction Rethrow { get; } = Create(OpCodes.Rethrow);

        public Instruction FinallyStart { get; } = Nop();

        public Instruction SaveReturnValue { get; } = Nop();

        public Instruction OnSuccess { get; } = Nop();

        public Instruction IfSuccessRetry { get; } = Nop();

        public Instruction IfSuccessReplaced { get; } = Nop();

        public Instruction OnExit { get; } = Nop();

        public Instruction EndFinally { get; } = Create(OpCodes.Endfinally);

        public Instruction FinallyEnd { get; set; } = Nop();

        public Instruction Ret { get; set; } = Create(OpCodes.Ret);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Instruction Nop() => Create(OpCodes.Nop);
    }
}
