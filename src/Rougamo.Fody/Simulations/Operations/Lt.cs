using Mono.Cecil.Cil;

namespace Rougamo.Fody.Simulations.Operations
{
    internal class Lt(ILoadable value1, ILoadable value2) : ComparableOperation(value1, value2)
    {
        public override OpCode TrueToken => OpCodes.Blt;

        public override OpCode FalseToken => OpCodes.Bge;
    }
}
