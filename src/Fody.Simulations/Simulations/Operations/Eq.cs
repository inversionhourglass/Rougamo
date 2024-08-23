using Mono.Cecil.Cil;

namespace Fody.Simulations.Operations
{
    public class Eq(ILoadable value1, ILoadable value2) : ComparableOperation(value1, value2)
    {
        public override OpCode TrueToken => OpCodes.Beq;

        public override OpCode FalseToken => OpCodes.Bne_Un;
    }
}
