using Mono.Cecil.Cil;

namespace Fody.Simulations
{
    public interface IAssertable
    {
        OpCode TrueToken { get; }

        OpCode FalseToken { get; }
    }
}
