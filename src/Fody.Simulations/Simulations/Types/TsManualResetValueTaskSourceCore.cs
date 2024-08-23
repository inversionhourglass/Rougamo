using Mono.Cecil;

namespace Fody.Simulations.Types
{
    public class TsManualResetValueTaskSourceCore(TypeReference typeRef, IHost? host, SimulationModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
    {
        public MethodSimulation M_SetException => MethodSimulate(Constants.METHOD_SetException, false);

        public MethodSimulation M_SetResult => MethodSimulate(Constants.METHOD_SetResult, false);
    }
}
