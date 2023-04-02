using Mono.Cecil;
using System.Linq;

namespace Rougamo.Fody.Enhances.Async
{
    internal class AsyncFields : IStateMachineFields
    {
        public AsyncFields(
            TypeDefinition stateMachineTypeDef,
            FieldDefinition? moArray, FieldDefinition[] mos,
            FieldDefinition methodContext,
            FieldDefinition state, FieldDefinition builder,
            FieldDefinition?[] parameters)
        {
            if (stateMachineTypeDef.HasGenericParameters)
            {
                // public async Task<MyClass<T>> Mt<T>()
                var stateTypeRef = new GenericInstanceType(stateMachineTypeDef);
                foreach (var parameter in stateMachineTypeDef.GenericParameters)
                {
                    stateTypeRef.GenericArguments.Add(parameter);
                }
                MoArray = moArray == null ? null : new FieldReference(moArray.Name, moArray.FieldType, stateTypeRef);
                Mos = mos.Select(x => new FieldReference(x.Name, x.FieldType, stateTypeRef)).ToArray();
                MethodContext = new FieldReference(methodContext.Name, methodContext.FieldType, stateTypeRef);
                State = new FieldReference(state.Name, state.FieldType, stateTypeRef);
                Builder = new FieldReference(builder.Name, builder.FieldType, stateTypeRef);
                Parameters = parameters.Select(x => x == null ? null : new FieldReference(x.Name, x.FieldType, stateTypeRef)).ToArray();
            }
            else
            {
                MoArray = moArray;
                Mos = mos;
                MethodContext = methodContext;
                State = state;
                Builder = builder;
                Parameters = parameters;
            }
        }

        public FieldReference? MoArray { get; }

        public FieldReference[] Mos { get; }

        public FieldReference MethodContext { get; }

        public FieldReference State { get; }

        public FieldReference Builder { get; }

        public FieldReference?[] Parameters { get; }
    }
}
