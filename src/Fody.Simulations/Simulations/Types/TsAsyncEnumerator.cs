using Mono.Cecil;

namespace Fody.Simulations.Types
{
    public class TsAsyncEnumerator(TypeReference typeRef, IHost? host, SimulationModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
    {
        public PropertySimulation P_Current => PropertySimulate(Constants.PROP_Current, false);

        public MethodSimulation M_MoveNextAsync => MethodSimulate(Constants.METHOD_MoveNextAsync, false);
    }
}
