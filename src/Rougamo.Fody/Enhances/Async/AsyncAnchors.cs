using Mono.Cecil.Cil;
using System.Runtime.CompilerServices;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Enhances.Async
{
    internal class AsyncAnchors : IAnchors
    {
        public AsyncAnchors(Instruction hostsBuilderCreate, Instruction catchStart, Instruction hostsSetException, Instruction hostsLeaveCatch, Instruction? hostsSetResult, Instruction? hostsLdlocReturn)
        {
            HostsBuilderCreate = hostsBuilderCreate;
            CatchStart = catchStart;
            HostsSetException = hostsSetException;
            HostsLeaveCatch = hostsLeaveCatch;
            HostsSetResult = hostsSetResult;
            HostsLdlocReturn = hostsLdlocReturn;
        }

        #region Origin Method Body

        public Instruction HostsBuilderCreate { get; }

        public Instruction InitMos { get; } = Nop();

        public Instruction InitContext { get; } = Nop();

        #endregion Origin Method Body

        #region StateMachine Method Body

        public Instruction IfFirstTimeEntry { get; } = Nop();

        public Instruction OnEntry { get; } = Nop();

        public Instruction IfEntryReplaced { get; } = Nop();

        public Instruction RewriteArg { get; } = Nop();

        public Instruction Retry { get; } = Nop();

        public Instruction CatchStart { get; }

        public Instruction SaveException { get; } = Nop();

        public Instruction OnExceptionRefreshArgs { get; set; } = Nop();

        public Instruction OnException { get; } = Nop();

        public Instruction IfExceptionRetry { get; } = Nop();

        public Instruction ExceptionContextStash { get; } = Nop();

        public Instruction OnExitAfterException { get; } = Nop();

        public Instruction IfExceptionHandled { get; } = Nop();

        public Instruction HostsSetException { get; }

        public Instruction HostsLeaveCatch { get; }

        public Instruction SaveReturnValue { get; } = Nop();

        public Instruction OnSuccessRefreshArgs { get; set; } = Nop();

        public Instruction OnSuccess { get; } = Nop();

        public Instruction IfSuccessRetry { get; } = Nop();

        public Instruction IfSuccessReplaced { get; } = Nop();

        public Instruction OnExitAfterSuccess { get; } = Nop();

        public Instruction? HostsSetResult { get; }

        public Instruction? HostsLdlocReturn { get; }

        #endregion StateMachine Method Body

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Instruction Nop() => Create(OpCodes.Nop);
    }
}
