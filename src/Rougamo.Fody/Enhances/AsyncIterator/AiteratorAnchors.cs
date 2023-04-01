using Mono.Cecil.Cil;
using System.Runtime.CompilerServices;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Enhances.AsyncIterator
{
    internal class AiteratorAnchors
    {
        public AiteratorAnchors(Instruction hostsReturn, Instruction hostsStart, Instruction hostsCatchStart, Instruction hostsSetException, Instruction finishReturn, Instruction? yieldReturn)
        {
            HostsReturn = hostsReturn;
            HostsStart = hostsStart;
            HostsCatchStart = hostsCatchStart;
            HostsSetException = hostsSetException;
            FinishReturn = finishReturn;
            YieldReturn = yieldReturn;
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

        public Instruction HostsStart { get; }

        public Instruction HostsCatchStart { get; }

        public Instruction SaveException { get; } = Nop();

        public Instruction OnException { get; } = Nop();

        public Instruction OnExitAfterException { get; } = Nop();

        public Instruction HostsSetException { get; }

        public Instruction FinishReturn { get; }

        public Instruction SaveReturnValue { get; } = Nop();

        public Instruction OnSuccess { get; } = Nop();

        public Instruction OnExitAfterSuccess { get; } = Nop();

        public Instruction FinishSetResultLdarg0 { get; } = Create(OpCodes.Ldarg_0);

        public Instruction? YieldReturn { get; }

        public Instruction SaveYieldReturnValue { get; } = Nop();

        public Instruction YieldSetResultLdarg0 { get; } = Create(OpCodes.Ldarg_0);

        #endregion StateMachine Method Body

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Instruction Nop() => Create(OpCodes.Nop);
    }
}
