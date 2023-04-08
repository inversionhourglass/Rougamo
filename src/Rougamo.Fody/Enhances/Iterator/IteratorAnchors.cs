using Mono.Cecil.Cil;
using System.Runtime.CompilerServices;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Enhances.Iterator
{
    internal class IteratorAnchors : ITryCatchFinallyAnchors, IAnchors
    {
        public IteratorAnchors(IteratorVariables variables, Instruction hostsReturn, Instruction tryStart, Instruction finallyEnd)
        {
            HostsReturn = hostsReturn;
            TryStart = tryStart;
            CatchStart = Create(OpCodes.Stloc, variables.Exception);
            FinallyEnd = finallyEnd;
        }

        #region Origin Method Body

        public Instruction InitMos { get; } = Nop();

        public Instruction InitContext { get; } = Nop();

        public Instruction InitRecordedReturn { get; } = Nop();

        public Instruction HostsReturn { get; }

        #endregion Origin Method Body

        #region StateMachine Method Body

        public Instruction IfFirstTimeEntry { get; } = Nop();

        public Instruction OnEntry { get; } = Nop();

        public Instruction RewriteArg { get; } = Nop();

        public Instruction TryStart { get; }

        public Instruction CatchStart { get; }

        public Instruction OnException { get; } = Nop();

        public Instruction OnExitAfterException { get; } = Nop();

        public Instruction Rethrow { get; } = Create(OpCodes.Rethrow);

        public Instruction FinallyStart { get; } = Nop();

        public Instruction IfLastYeild { get; } = Nop();

        public Instruction IfHasException { get; } = Nop();

        public Instruction SaveReturnValue { get; } = Nop();

        public Instruction OnSuccess { get; } = Nop();

        public Instruction OnExitAfterSuccess { get; } = Nop();

        public Instruction EndFinally { get; } = Create(OpCodes.Endfinally);

        public Instruction FinallyEnd { get; }

        #endregion StateMachine Method Body

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Instruction Nop() => Create(OpCodes.Nop);
    }
}
