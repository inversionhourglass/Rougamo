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
            FieldDefinition? declaringThis, FieldDefinition?[] parameters)
        {
            if (stateMachineTypeDef.HasGenericParameters)
            {
                // public async Task<MyClass<T>> Mt<T>()
                DeclaringTypeRef = new GenericInstanceType(stateMachineTypeDef);
                foreach (var parameter in stateMachineTypeDef.GenericParameters)
                {
                    DeclaringTypeRef.GenericArguments.Add(parameter);
                }
                MoArray = moArray == null ? null : new FieldReference(moArray.Name, moArray.FieldType, DeclaringTypeRef);
                Mos = mos.Select(x => new FieldReference(x.Name, x.FieldType, DeclaringTypeRef)).ToArray();
                MethodContext = new FieldReference(methodContext.Name, methodContext.FieldType, DeclaringTypeRef);
                State = new FieldReference(state.Name, state.FieldType, DeclaringTypeRef);
                Builder = new FieldReference(builder.Name, builder.FieldType, DeclaringTypeRef);
                DeclaringThis = declaringThis == null ? null : new FieldReference(declaringThis.Name, declaringThis.FieldType, DeclaringTypeRef);
                Parameters = parameters.Select(x => x == null ? null : new FieldReference(x.Name, x.FieldType, DeclaringTypeRef)).ToArray();
            }
            else
            {
                MoArray = moArray;
                Mos = mos;
                MethodContext = methodContext;
                State = state;
                Builder = builder;
                DeclaringThis = declaringThis;
                Parameters = parameters;
            }
        }

        public GenericInstanceType? DeclaringTypeRef { get; }

        public FieldReference? MoArray { get; }

        public FieldReference[] Mos { get; }

        public FieldReference MethodContext { get; }

        public FieldReference State { get; }

        public FieldReference Builder { get; }

        public FieldReference? DeclaringThis { get; }

        public FieldReference?[] Parameters { get; }

        private FieldReference? _awaiter;
        public FieldReference? Awaiter
        {
            get => _awaiter;
            set
            {
                if (value is FieldDefinition && DeclaringTypeRef != null)
                {
                    _awaiter = new FieldReference(value.Name, value.FieldType, DeclaringTypeRef);
                }
                else
                {
                    _awaiter = value;
                }
            }
        }

        public void SetParameter(int index, FieldDefinition fieldDef)
        {
            if (DeclaringTypeRef != null)
            {
                Parameters[index] = new FieldReference(fieldDef.Name, fieldDef.FieldType, DeclaringTypeRef);
            }
            else
            {
                Parameters[index] = fieldDef;
            }
        }
    }
}
