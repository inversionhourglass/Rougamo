using Mono.Cecil;

namespace Rougamo.Fody.Simulations.Types
{
    /// <summary>
    /// Any type which is the return type of <see cref="TsAsyncable"/>'s GetAwaiter method
    /// </summary>
    internal class TsAwaiter(TypeReference typeRef, IHost? host, ModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
    {
        private PropertySimulation? _pIsCompleted;

        public PropertySimulation P_IsCompleted => PropertySimulate(Constants.PROP_IsCompleted, true);

        public MethodSimulation M_GetResult => MethodSimulate(Constants.METHOD_GetResult, true);
    }
}
