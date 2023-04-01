using Mono.Cecil.Cil;

namespace Rougamo.Fody.Enhances.AsyncIterator
{
    internal class AiteratorVariables : IIteratorVariables
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
