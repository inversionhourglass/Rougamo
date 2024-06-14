using Mono.Cecil;

namespace Rougamo.Fody.Simulations
{
    internal class TsAsyncBuilder(TypeReference typeRef, ModuleDefinition moduleDef) : TypeSimulation(typeRef, moduleDef)
    {
        private MethodSimulations? _methods;

        public MethodSimulations Methods => _methods ??= new(this);

        public class MethodSimulations(TsAsyncBuilder declaringType)
        {
            private readonly TsAsyncBuilder _declaringType = declaringType;

            public MethodSimulation<TypeSimulation> AwaitUnsafeOnCompleted => _declaringType.PublicMethodSimulate<TypeSimulation>(Constants.METHOD_AwaitUnsafeOnCompleted);

            public MethodSimulation<TypeSimulation> SetException => _declaringType.PublicMethodSimulate<TypeSimulation>(Constants.METHOD_SetException);

            public MethodSimulation<TypeSimulation> SetResult => _declaringType.PublicMethodSimulate<TypeSimulation>(Constants.METHOD_SetResult);
        }
    }
}
