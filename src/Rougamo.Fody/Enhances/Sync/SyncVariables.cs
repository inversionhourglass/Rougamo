using Mono.Cecil.Cil;

namespace Rougamo.Fody.Enhances.Sync
{
    internal class SyncVariables
    {
        public SyncVariables(
            VariableDefinition? moArray, VariableDefinition[] mos,
            VariableDefinition methodContext, VariableDefinition isRetry,
            VariableDefinition exception)
        {
            MoArray = moArray;
            Mos = mos;
            MethodContext = methodContext;
            IsRetry = isRetry;
            Exception = exception;
        }

        public VariableDefinition? MoArray { get; set; }

        public VariableDefinition[] Mos { get; set; }

        public VariableDefinition MethodContext { get; set; }

        public VariableDefinition IsRetry { get; }

        public VariableDefinition Exception { get; }

        public VariableDefinition? Return { get; set; }
    }
}
