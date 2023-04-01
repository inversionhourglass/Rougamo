using Mono.Cecil.Cil;

namespace Rougamo.Fody.Enhances
{
    internal interface ITryCatchFinallyAnchors
    {
        Instruction TryStart { get; }

        Instruction CatchStart { get; }

        Instruction FinallyStart { get; }

        Instruction FinallyEnd { get; }
    }
}
