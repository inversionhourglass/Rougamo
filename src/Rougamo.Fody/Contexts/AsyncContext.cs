using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Contexts
{
    internal class AsyncContext(RouMethod rouMethod) : IAsyncContext
    {
        public bool OnExceptionAllInSync { get; } = rouMethod.Mos.All(x => x.ForceSync.Contains(ForceSync.OnException));

        public bool OnExitAllInSync { get; } = rouMethod.Mos.All(x => x.ForceSync.Contains(ForceSync.OnExit));

        public int State { get; set; } = 0;

        public bool AwaitFirst { get; set; } = false;

        public List<Instruction> AnchorSwitches { get; } = [];

        public Instruction AnchorProxyCallCase { get; } = Instruction.Create(OpCodes.Nop);

        public Instruction AnchorOnExceptionAsync { get; } = Instruction.Create(OpCodes.Nop);

        public Instruction AnchorOnExitAsync { get; } = Instruction.Create(OpCodes.Nop);

        public Instruction AnchorStateReady { get; private set; } = Instruction.Create(OpCodes.Nop);

        public Instruction AnchorSetResult { get; } = Instruction.Create(OpCodes.Nop);

        public Instruction Ret { get; } = Instruction.Create(OpCodes.Ret);

        public Instruction GetNextStateReadyAnchor() => AnchorStateReady = Instruction.Create(OpCodes.Nop);
    }
}
