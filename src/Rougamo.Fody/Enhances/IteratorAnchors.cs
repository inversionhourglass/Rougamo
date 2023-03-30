using Mono.Cecil.Cil;
using System.Runtime.CompilerServices;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Enhances
{
    internal class IteratorAnchors
    {
        public IteratorAnchors(IteratorVariables variables, Instruction @return, Instruction tryStart, Instruction finallyEnd)
        {
            Return = @return;
            TryStart = tryStart;
            CatchStart = Create(OpCodes.Stloc, variables.Exception);
            FinallyEnd = finallyEnd;
        }

        #region Origin Method Body

        public Instruction InitMosStart { get; } = Nop();

        public Instruction InitContextStart { get; } = Nop();

        public Instruction InitRecordedReturnStart { get; } = Nop();

        public Instruction Return { get; }

        #endregion Origin Method Body

        #region StateMachine Method Body

        public Instruction OnEntryStart { get; } = Nop();

        public Instruction RewriteArgStart { get; } = Nop();

        public Instruction TryStart { get; }

        public Instruction CatchStart { get; }

        public Instruction OnExceptionStart { get; } = Nop();

        public Instruction Rethrow { get; } = Create(OpCodes.Rethrow);

        public Instruction FinallyStart { get; } = Nop();

        public Instruction IfLastYeildStart { get; } = Nop();

        public Instruction IfHasExceptionStart { get; } = Nop();

        public Instruction SaveReturnValueStart { get; } = Nop();

        public Instruction OnSuccessStart { get; } = Nop();

        public Instruction OnExitStart { get; } = Nop();

        public Instruction EndFinally { get; } = Create(OpCodes.Endfinally);

        public Instruction FinallyEnd { get; }

        #endregion StateMachine Method Body

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Instruction Nop() => Create(OpCodes.Nop);
    }
}
