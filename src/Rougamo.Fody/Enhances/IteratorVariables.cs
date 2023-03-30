using Mono.Cecil.Cil;

namespace Rougamo.Fody.Enhances
{
    internal class IteratorVariables
    {
        public IteratorVariables(VariableDefinition stateMachine, VariableDefinition exception, VariableDefinition moveNextReturn)
        {
            StateMachine = stateMachine;
            Exception = exception;
            MoveNextReturn = moveNextReturn;
        }

        public VariableDefinition StateMachine { get; }

        public VariableDefinition Exception { get; }

        public VariableDefinition MoveNextReturn { get; }
    }
}
