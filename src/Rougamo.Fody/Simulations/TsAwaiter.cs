using Mono.Cecil;

namespace Rougamo.Fody.Simulations
{
    /// <summary>
    /// Any type which is the return type of <see cref="TsAsyncable"/>'s GetAwaiter method
    /// </summary>
    internal class TsAwaiter : TypeSimulation
    {
        public TsAwaiter(TypeReference typeRef, GenericParameter[]? generics, ModuleDefinition moduleDef) : base(typeRef, generics, moduleDef) { }

        public MsGetterIsCompleted PGIsCompleted => MethodSimulate<MsGetterIsCompleted>(nameof(PGIsCompleted), Constants.Getter(Constants.PROP_IsCompleted));

        public MsGetResult MGetResult => MethodSimulate<MsGetResult>(nameof(MGetResult), Constants.METHOD_GetResult);

        public class MsGetterIsCompleted : MethodSimulation
        {
            public MsGetterIsCompleted(TypeSimulation declaringType, MethodDefinition methodDef) : base(declaringType, methodDef) { }
        }

        public class MsGetResult : MethodSimulation
        {
            public MsGetResult(TypeSimulation declaringType, MethodDefinition methodDef) : base(declaringType, methodDef) { }
        }
    }
}
