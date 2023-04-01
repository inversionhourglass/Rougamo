using Mono.Cecil.Cil;

namespace Rougamo.Fody.Enhances
{
    internal interface IStateMachineVariables
    {
        VariableDefinition StateMachine { get; }
    }
}
