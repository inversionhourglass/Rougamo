using Mono.Cecil.Cil;

namespace Rougamo.Fody.Enhances.Iterator
{
    internal class IteratorVariables : IIteratorVariables
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
