using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsEnumerator(TypeReference typeRef, IHost? host, ModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
    {
        public PropertySimulation P_Current => PropertySimulate(Constants.PROP_Current, false);

        public MethodSimulation M_MoveNext => MethodSimulate(Constants.METHOD_MoveNext, true);
    }
}
