using Mono.Cecil;

namespace Fody.Simulations.Types
{
    public class TsEnumerable(TypeReference typeRef, IHost? host, SimulationModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
    {
        public MethodSimulation<TsEnumerator> M_GetEnumerator => MethodSimulate<TsEnumerator>(Constants.METHOD_GetEnumerator, false);
    }
}
