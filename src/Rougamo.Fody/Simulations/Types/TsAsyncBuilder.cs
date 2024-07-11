using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
    internal class TsAsyncBuilder(TypeReference typeRef, IHost? host, ModuleDefinition moduleDef) : TypeSimulation(typeRef, host, moduleDef)
    {
        public MethodSimulation M_AwaitUnsafeOnCompleted => PublicMethodSimulate(Constants.METHOD_AwaitUnsafeOnCompleted);

        public MethodSimulation M_SetException => PublicMethodSimulate(Constants.METHOD_SetException);

        public MethodSimulation M_SetResult => PublicMethodSimulate(Constants.METHOD_SetResult);
    }
}
