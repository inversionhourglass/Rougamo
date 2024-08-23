using Mono.Cecil;

namespace Fody.Simulations.Types
{
    /// <summary>
    /// Any type which support async syntax. etc: Task, ValueTask, Task&lt;T&gt;, ValueTask&lt;T&gt;
    /// </summary>
    public class TsAwaitable(TypeReference typeRef, IHost? host, SimulationModuleWeaver moduldeWeaver) : TypeSimulation(typeRef, host, moduldeWeaver)
    {
        public MethodSimulation<TsAwaiter> M_GetAwaiter => MethodSimulate<TsAwaiter>(Constants.METHOD_GetAwaiter, true);
    }
}
