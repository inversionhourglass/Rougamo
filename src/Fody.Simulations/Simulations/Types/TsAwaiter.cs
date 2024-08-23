using Mono.Cecil;

namespace Fody.Simulations.Types
{
    /// <summary>
    /// Any type which is the return type of <see cref="TsAwaitable"/>'s GetAwaiter method
    /// </summary>
    public class TsAwaiter(TypeReference typeRef, IHost? host, SimulationModuleWeaver moduleWeaver) : TypeSimulation(typeRef, host, moduleWeaver)
    {
        private PropertySimulation? _pIsCompleted;

        public PropertySimulation P_IsCompleted => PropertySimulate(Constants.PROP_IsCompleted, true);

        public MethodSimulation M_GetResult => MethodSimulate(Constants.METHOD_GetResult, true);
    }
}
