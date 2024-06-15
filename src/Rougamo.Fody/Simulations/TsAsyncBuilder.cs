using Mono.Cecil;

namespace Rougamo.Fody.Simulations
{
    internal class TsAsyncBuilder(TypeReference typeRef, ModuleDefinition moduleDef) : TypeSimulation(typeRef, moduleDef)
    {
        public MethodSimulation<TypeSimulation> MAwaitUnsafeOnCompleted => PublicMethodSimulate<TypeSimulation>(Constants.METHOD_AwaitUnsafeOnCompleted);

        public MethodSimulation<TypeSimulation> MSetException => PublicMethodSimulate<TypeSimulation>(Constants.METHOD_SetException);

        public MethodSimulation<TypeSimulation> MSetResult => PublicMethodSimulate<TypeSimulation>(Constants.METHOD_SetResult);
    }
}
