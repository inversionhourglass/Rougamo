using Mono.Cecil.Cil;

namespace Rougamo.Fody.Enhances.Async
{
    internal class ProxyAsyncVariables
    {
        public ProxyAsyncVariables(VariableDefinition state, VariableDefinition? @this, VariableDefinition? result)
        {
            State = state;
            This = @this;
            Result = result;
        }

        public VariableDefinition State { get; }

        public VariableDefinition? This { get; }

        public VariableDefinition? Result { get; }
    }
}
