using Mono.Cecil.Cil;

namespace Rougamo.Fody.Enhances
{
    internal class AsyncVariables
    {
        public AsyncVariables(VariableDefinition stateMachine, VariableDefinition? replacedReturn, VariableDefinition exceptionHandled)
        {
            StateMachine = stateMachine;
            ReplacedReturn = replacedReturn;
            ExceptionHandled = exceptionHandled;
        }

        public VariableDefinition StateMachine { get; }

        public VariableDefinition? ReplacedReturn { get; }

        public VariableDefinition ExceptionHandled { get; }
    }
}
