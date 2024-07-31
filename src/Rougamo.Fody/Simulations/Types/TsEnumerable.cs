using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsEnumerable(TypeReference typeRef, IHost? host, ModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
    {
        public MethodSimulation<TsEnumerator> M_GetEnumerator => MethodSimulate<TsEnumerator>(Constants.METHOD_GetEnumerator, false);
    }
}
