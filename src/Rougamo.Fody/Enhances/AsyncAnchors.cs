using Mono.Cecil.Cil;
using System.Runtime.CompilerServices;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Enhances
{
    internal class AsyncAnchors
    {
        public AsyncAnchors(Instruction builderCreateStart, Instruction catchStart, Instruction setExceptionStart, Instruction leaveCatch, Instruction? builderSetResultStart, Instruction? ldlocReturn)
        {
            BuilderCreateStart = builderCreateStart;
            CatchStart = catchStart;
            SetExceptionStart = setExceptionStart;
            LeaveCatch = leaveCatch;
            BuilderSetResultStart = builderSetResultStart;
            LdlocReturn = ldlocReturn;
        }

        #region Origin Method Body

        public Instruction BuilderCreateStart { get; }

        public Instruction InitMosStart { get; } = Nop();

        public Instruction InitContextStart { get; } = Nop();

        #endregion Origin Method Body

        #region StateMachine Method Body

        public Instruction OnEntryStart { get; } = Nop();

        public Instruction IfEntryReplacedStart { get; } = Nop();

        public Instruction RewriteArgStart { get; } = Nop();

        public Instruction RetryStart { get; } = Nop();

        public Instruction CatchStart { get; }

        public Instruction SaveException { get; } = Nop();

        public Instruction OnExceptionStart { get; } = Nop();

        public Instruction IfExceptionRetryStart { get; } = Nop();

        public Instruction ExceptionContextStashStart { get; } = Nop();

        public Instruction OnExitAfterExceptionHandledStart { get; } = Nop();

        public Instruction IfExceptionHandled { get; } = Nop();

        public Instruction SetExceptionStart { get; }

        public Instruction LeaveCatch { get; }

        public Instruction SaveReturnValueStart { get; } = Nop();

        public Instruction OnSuccessStart { get; } = Nop();

        public Instruction IfSuccessRetryStart { get; } = Nop();

        public Instruction IfSuccessReplacedStart { get; } = Nop();

        public Instruction OnExitAfterSuccessExecutedStart { get; } = Nop();

        public Instruction? BuilderSetResultStart { get; }

        public Instruction? LdlocReturn { get; }

        #endregion StateMachine Method Body

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Instruction Nop() => Create(OpCodes.Nop);
    }
}
