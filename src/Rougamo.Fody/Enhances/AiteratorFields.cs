using Mono.Cecil;
using System.Linq;

namespace Rougamo.Fody.Enhances
{
    internal class AiteratorFields
    {
        public AiteratorFields(
            TypeDefinition stateMachineTypeDef,
            FieldDefinition mos, FieldDefinition methodContext,
            FieldDefinition state, FieldDefinition current,
            FieldDefinition? recordedReturn, FieldDefinition?[] parameters)
        {
            if (stateMachineTypeDef.HasGenericParameters)
            {
                // public IEnumerable<MyClass<T>> Mt<T>()
                var stateTypeRef = new GenericInstanceType(stateMachineTypeDef);
                foreach (var parameter in stateMachineTypeDef.GenericParameters)
                {
                    stateTypeRef.GenericArguments.Add(parameter);
                }
                Mos = new FieldReference(mos.Name, mos.FieldType, stateTypeRef);
                MethodContext = new FieldReference(methodContext.Name, methodContext.FieldType, stateTypeRef);
                State = new FieldReference(state.Name, state.FieldType, stateTypeRef);
                Current = new FieldReference(current.Name, current.FieldType, stateTypeRef);
                RecordedReturn = recordedReturn == null ? null : new FieldReference(recordedReturn.Name, recordedReturn.FieldType, stateTypeRef);
                Parameters = parameters.Select(x => x == null ? null : new FieldReference(x.Name, x.FieldType, stateTypeRef)).ToArray();
            }
            else
            {
                Mos = mos;
                MethodContext = methodContext;
                State = state;
                Current = current;
                RecordedReturn = recordedReturn;
                Parameters = parameters;
            }
        }

        public FieldReference Mos { get; }

        public FieldReference MethodContext { get; }

        public FieldReference State { get; }

        public FieldReference Current { get; }

        public FieldReference? RecordedReturn { get; }

        public FieldReference?[] Parameters { get; }
    }
}
