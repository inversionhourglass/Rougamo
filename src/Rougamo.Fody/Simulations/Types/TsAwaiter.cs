using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
    /// <summary>
    /// Any type which is the return type of <see cref="TsAsyncable"/>'s GetAwaiter method
    /// </summary>
    internal class TsAwaiter(TypeReference typeRef, ModuleDefinition moduleDef) : TypeSimulation(typeRef, moduleDef)
    {
        private PropertySimulation<TypeSimulation>? _pIsCompleted;

        public PropertySimulation<TypeSimulation> PIsCompleted => _pIsCompleted ??= new(Constants.PROP_IsCompleted, this);

        public MethodSimulation<TypeSimulation> MGetResult => MethodSimulate<TypeSimulation>(Constants.METHOD_GetResult);
    }
}
