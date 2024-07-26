using Mono.Cecil.Cil;

namespace Rougamo.Fody.Simulations.Asserters
{
    internal class Gt(ILoadable value1, ILoadable value2) : ComparableAsserter(value1, value2)
    {
        public override OpCode TrueToken => OpCodes.Bgt;

        public override OpCode FalseToken => OpCodes.Ble;
    }
}
