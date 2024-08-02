using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsAsyncIteratorMethodBuilder(TypeReference typeRef, IHost? host, ModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver), IAsyncBuilder
    {
        public MethodSimulation M_AwaitUnsafeOnCompleted => PublicMethodSimulate(Constants.METHOD_AwaitUnsafeOnCompleted, false);

        public MethodSimulation M_Complete => PublicMethodSimulate(Constants.METHOD_Complete, false);
    }
}
