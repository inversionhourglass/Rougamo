using Mono.Cecil;

namespace Fody.Simulations.Types
{
    public class TsAsyncEnumerable(TypeReference typeRef, IHost? host, SimulationModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
    {
        public MethodSimulation<TsEnumerator> M_GetAsyncEnumerator => MethodSimulate<TsEnumerator>(Constants.METHOD_GetAsyncEnumerator, false);
    }
}
