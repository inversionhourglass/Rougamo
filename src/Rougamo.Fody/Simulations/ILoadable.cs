using Mono.Cecil.Cil;

namespace Rougamo.Fody.Simulations
{
    internal interface ILoadable
    {
        Instruction[]? Load();

        Instruction[]? LoadForCallingMethod();
    }
}
