using Mono.Cecil.Cil;

namespace Rougamo.Fody.Simulations
{
    internal interface ICallingTarget
    {
        Instruction[]? LoadForCallingMethod();
    }
}
