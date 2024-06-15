using Mono.Cecil;

namespace Rougamo.Fody.Simulations
{
    /// <summary>
    /// Any type which support async syntax. etc: Task, ValueTask, Task&lt;T&gt;, ValueTask&lt;T&gt;
    /// </summary>
    internal class TsAsyncable(TypeReference typeRef, ModuleDefinition moduldeDef) : TypeSimulation(typeRef, moduldeDef)
    {
        public MethodSimulation<TsAwaiter> MGetAwaiter => MethodSimulate<TsAwaiter>(Constants.METHOD_GetAwaiter);
    }
}
