using Mono.Cecil.Cil;

namespace Rougamo.Fody.Enhances
{
    internal class AiteratorVariables
    {
        public AiteratorVariables(VariableDefinition stateMachine, VariableDefinition moveNextReturn)
        {
            StateMachine = stateMachine;
            MoveNextReturn = moveNextReturn;
        }

        public VariableDefinition StateMachine { get; }

        public VariableDefinition MoveNextReturn { get; }
    }
}
