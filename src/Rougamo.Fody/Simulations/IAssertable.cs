using Mono.Cecil.Cil;

namespace Rougamo.Fody.Simulations
{
    internal interface IAssertable
    {
        OpCode TrueToken { get; }

        OpCode FalseToken { get; }
    }
}
