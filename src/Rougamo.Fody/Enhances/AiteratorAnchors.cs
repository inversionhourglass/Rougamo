using Mono.Cecil.Cil;
using System.Runtime.CompilerServices;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Enhances
{
    internal class AiteratorAnchors
    {
        public AiteratorAnchors(Instruction @return, Instruction originStart, Instruction catchStart, Instruction setExceptionStart, Instruction finishReturnStart, Instruction? yieldReturnStart)
        {
            Return = @return;
            OriginStart = originStart;
            CatchStart = catchStart;
            SetExceptionStart = setExceptionStart;
            FinishReturnStart = finishReturnStart;
            YieldReturnStart = yieldReturnStart;
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

        public Instruction OriginStart { get; }

        public Instruction CatchStart { get; }

        public Instruction SaveException { get; } = Nop();

        public Instruction OnExceptionStart { get; } = Nop();

        public Instruction OnExitAfterExceptionStart { get; } = Nop();

        public Instruction SetExceptionStart { get; }

        public Instruction FinishReturnStart { get; }

        public Instruction SaveReturnValueStart { get; } = Nop();

        public Instruction OnSuccessStart { get; } = Nop();

        public Instruction OnExitAfterSuccessStart { get; } = Nop();

        public Instruction FinishSetResultStart { get; } = Create(OpCodes.Ldarg_0);

        public Instruction? YieldReturnStart { get; }

        public Instruction SaveYieldReturnValueStart { get; } = Nop();

        public Instruction YieldSetResultStart { get; } = Create(OpCodes.Ldarg_0);

        #endregion StateMachine Method Body

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Instruction Nop() => Create(OpCodes.Nop);
    }
}
