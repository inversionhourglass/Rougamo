using Mono.Cecil.Cil;

namespace Rougamo.Fody.Enhances.Async
{
    internal class AsyncVariables : IStateMachineVariables
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
