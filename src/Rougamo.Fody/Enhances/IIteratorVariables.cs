using Mono.Cecil.Cil;

namespace Rougamo.Fody.Enhances
{
    internal interface IIteratorVariables : IStateMachineVariables
    {
        VariableDefinition MoveNextReturn { get; }
    }
}
