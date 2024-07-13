using Mono.Cecil.Cil;

namespace Rougamo.Fody.Simulations.Asserters
{
    internal class Eq(ILoadable value1, ILoadable value2) : ComparableAsserter(value1, value2)
    {
        public override OpCode TrueToken => OpCodes.Beq;

        public override OpCode FalseToken => OpCodes.Bne_Un;
    }
}
