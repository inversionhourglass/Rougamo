using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsManualResetValueTaskSourceCore(TypeReference typeRef, IHost? host, ModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
    {
        public MethodSimulation M_SetException => MethodSimulate(Constants.METHOD_SetException, false);

        public MethodSimulation M_SetResult => MethodSimulate(Constants.METHOD_SetResult, false);
    }
}
