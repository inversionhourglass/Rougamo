using Mono.Cecil.Cil;

namespace Rougamo.Fody.Enhances.Sync
{
    internal class SyncVariables
    {
        public SyncVariables(
            VariableDefinition mos, VariableDefinition methodContext,
            VariableDefinition isRetry, VariableDefinition exception)
        {
            Mos = mos;
            MethodContext = methodContext;
            IsRetry = isRetry;
            Exception = exception;
        }

        public VariableDefinition Mos { get; set; }

        public VariableDefinition MethodContext { get; set; }

        public VariableDefinition IsRetry { get; }

        public VariableDefinition Exception { get; }

        public VariableDefinition? Return { get; set; }
    }
}
