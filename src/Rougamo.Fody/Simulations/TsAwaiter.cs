using Mono.Cecil;

namespace Rougamo.Fody.Simulations
{
    /// <summary>
    /// Any type which is the return type of <see cref="TsAsyncable"/>'s GetAwaiter method
    /// </summary>
    internal class TsAwaiter(TypeReference typeRef, ModuleDefinition moduleDef) : TypeSimulation(typeRef, moduleDef)
    {
        private PropertySimulations? _properties;
        private MethodSimulations? _methods;

        public PropertySimulations Properties => _properties ??= new(this);

        public MethodSimulations Methods => _methods ??= new(this);

        public class MethodSimulations(TsAwaiter declaringType)
        {
            private readonly TsAwaiter _declaringType = declaringType;

            public MethodSimulation<TypeSimulation> GetResult => _declaringType.MethodSimulate<TypeSimulation>(Constants.METHOD_GetResult);
        }

        public class PropertySimulations(TsAwaiter declaringType)
        {
            public PropertySimulation<TypeSimulation> IsCompleted { get; } = new(Constants.PROP_IsCompleted, declaringType);
        }
    }
}
