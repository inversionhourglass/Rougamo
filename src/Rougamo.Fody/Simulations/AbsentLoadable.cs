using Mono.Cecil.Cil;

namespace Rougamo.Fody.Simulations
{
    internal class AbsentLoadable : ILoadable
    {
        public Instruction[]? Load() => null;

        public Instruction[]? LoadForCallingMethod() => null;
    }
}
