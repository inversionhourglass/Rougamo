using Mono.Cecil.Cil;

namespace Rougamo.Fody.Simulations.Operations
{
    internal class Gt(ILoadable value1, ILoadable value2) : ComparableOperation(value1, value2)
    {
        public override OpCode TrueToken => OpCodes.Bgt;

        public override OpCode FalseToken => OpCodes.Ble;
    }
}
