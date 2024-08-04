using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Rougamo.Fody.Contexts
{
    internal interface IAsyncContext
    {
        int State { get; set; }

        bool AwaitFirst { get; set; }

        List<Instruction> AnchorSwitches { get; }

        Instruction AnchorStateReady { get; }

        Instruction AnchorOnExceptionAsync { get; }

        Instruction AnchorOnExitAsync { get; }

        Instruction Ret { get; }

        Instruction GetNextStateReadyAnchor();
    }
}
