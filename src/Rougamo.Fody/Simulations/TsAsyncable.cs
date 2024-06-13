using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Simulations
{
    /// <summary>
    /// Any type which support async syntax. etc: Task, ValueTask, Task&lt;T&gt;, ValueTask&lt;T&gt;
    /// </summary>
    internal class TsAsyncable(TypeReference typeRef, Dictionary<string, GenericParameter>? genericMap, ModuleDefinition moduldeDef) : TypeSimulation(typeRef, genericMap, moduldeDef)
    {
        private MethodSimulations? _methods;

        public MethodSimulations Methods => _methods ??= new(this);

        public class MethodSimulations
        {
            private readonly TsAsyncable _declaringType;

            public MethodSimulations(TsAsyncable declaringType)
            {
                _declaringType = declaringType;
            }

            public MsGetAwaiter GetAwaiter => _declaringType.MethodSimulate<MsGetAwaiter>(nameof(GetAwaiter), Constants.METHOD_GetAwaiter);
        }

        public class MsGetAwaiter(TypeSimulation declaringType, MethodDefinition methodDef) : MethodSimulation(declaringType, methodDef)
        {
            private TsAwaiter? _returnType;

            public TsAwaiter ReturnType => _returnType ??= Def.ReturnType.Simulate<TsAwaiter>(Module, ((GenericInstanceType)_declaringType.Ref).GenericArguments.ToArray());
        }
    }
}
