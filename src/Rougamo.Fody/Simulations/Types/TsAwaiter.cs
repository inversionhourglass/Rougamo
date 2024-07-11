using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
    /// <summary>
    /// Any type which is the return type of <see cref="TsAsyncable"/>'s GetAwaiter method
    /// </summary>
    internal class TsAwaiter(TypeReference typeRef, IHost? host, ModuleDefinition moduleDef) : TypeSimulation(typeRef, host, moduleDef)
    {
        private PropertySimulation? _pIsCompleted;

        public PropertySimulation P_IsCompleted => _pIsCompleted ??= new(Constants.PROP_IsCompleted, this);

        public MethodSimulation M_GetResult => MethodSimulate(Constants.METHOD_GetResult);
    }
}
