using Mono.Cecil.Cil;
using static Mono.Cecil.Cil.Instruction;

namespace Rougamo.Fody.Simulations
{
    internal class LoadThis : ILoadable
    {
        public Instruction[]? Load()
        {
            return [Create(OpCodes.Ldarg_0)];
        }

        public Instruction[]? LoadForCallingMethod()
        {
            return [Create(OpCodes.Ldarg_0)];
        }
    }
}
