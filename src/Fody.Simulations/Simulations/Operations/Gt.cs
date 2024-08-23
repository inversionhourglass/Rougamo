using Mono.Cecil.Cil;

namespace Fody.Simulations.Operations
{
    public class Gt(ILoadable value1, ILoadable value2) : ComparableOperation(value1, value2)
    {
        public override OpCode TrueToken => OpCodes.Bgt;

        public override OpCode FalseToken => OpCodes.Ble;
    }
}
