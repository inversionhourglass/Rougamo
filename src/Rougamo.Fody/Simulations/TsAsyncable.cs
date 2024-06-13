using Mono.Cecil;

namespace Rougamo.Fody.Simulations
{
    /// <summary>
    /// Any type which support async syntax. etc: Task, ValueTask, Task&lt;T&gt;, ValueTask&lt;T&gt;
    /// </summary>
    internal class TsAsyncable(TypeReference typeRef, ModuleDefinition moduldeDef) : TypeSimulation(typeRef, moduldeDef)
    {
        private MethodSimulations? _methods;

        public MethodSimulations Methods => _methods ??= new(this);

        public class MethodSimulations(TsAsyncable declaringType)
        {
            private readonly TsAsyncable _declaringType = declaringType;

            public MethodSimulation<TsAwaiter> GetAwaiter => _declaringType.MethodSimulate<TsAwaiter>(Constants.METHOD_GetAwaiter);
        }
    }
}
