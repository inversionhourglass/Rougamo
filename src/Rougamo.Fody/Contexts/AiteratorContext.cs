using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Contexts
{
    internal class AiteratorContext(RouMethod rouMethod) : IAsyncContext
    {
        public bool OnExceptionAllInSync { get; } = rouMethod.Mos.All(x => x.ForceSync.Contains(ForceSync.OnException));

        public bool OnExitAllInSync { get; } = rouMethod.Mos.All(x => x.ForceSync.Contains(ForceSync.OnExit));

        public int State { get; set; } = 0;

        public bool AwaitFirst { get; set; } = false;

        public List<Instruction> AnchorSwitches { get; } = [];

        public Instruction AnchorStateReady { get; private set; } = Create(OpCodes.Nop);

        public Instruction AnchorSwitchDefault { get; } = Create(OpCodes.Nop);

        public Instruction AnchorSwitchM4 { get; } = Create(OpCodes.Nop);

        public Instruction AnchorSwitchMoveNext { get; } = Create(OpCodes.Nop);

        public Instruction AnchorOnExceptionAsync { get; } = Create(OpCodes.Nop);

        public Instruction AnchorOnExitAsync { get; } = Create(OpCodes.Nop);

        public Instruction AnchorEnd { get; } = Create(OpCodes.Nop);

        public Instruction AnchorYield { get; } = Create(OpCodes.Nop);

        public Instruction Ret { get; } = Create(OpCodes.Ret);

        public Instruction GetNextStateReadyAnchor() => AnchorStateReady = Create(OpCodes.Nop);
    }
}
