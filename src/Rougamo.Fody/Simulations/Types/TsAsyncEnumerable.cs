using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsAsyncEnumerable(TypeReference typeRef, IHost? host, ModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
    {
        public MethodSimulation<TsEnumerator> M_GetAsyncEnumerator => MethodSimulate<TsEnumerator>(Constants.METHOD_GetAsyncEnumerator, false);
    }
}
