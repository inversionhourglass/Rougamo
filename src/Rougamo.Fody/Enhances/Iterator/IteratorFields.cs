using Mono.Cecil;
using System;
using System.Linq;

namespace Rougamo.Fody.Enhances.Iterator
{
    internal class IteratorFields : IIteratorFields
    {
        public IteratorFields(
            TypeDefinition stateMachineTypeDef,
            FieldDefinition? moArray, FieldDefinition[] mos,
            FieldDefinition methodContext,
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
                MoArray = moArray == null ? null : new FieldReference(moArray.Name, moArray.FieldType, stateTypeRef);
                Mos = mos.Select(x => new FieldReference(x.Name, x.FieldType, stateTypeRef)).ToArray();
                MethodContext = new FieldReference(methodContext.Name, methodContext.FieldType, stateTypeRef);
                State = new FieldReference(state.Name, state.FieldType, stateTypeRef);
                Current = new FieldReference(current.Name, current.FieldType, stateTypeRef);
                RecordedReturn = recordedReturn == null ? null : new FieldReference(recordedReturn.Name, recordedReturn.FieldType, stateTypeRef);
                Parameters = parameters.Select(x => x == null ? null : new FieldReference(x.Name, x.FieldType, stateTypeRef)).ToArray();
            }
            else
            {
                MoArray = moArray;
                Mos = mos;
                MethodContext = methodContext;
                State = state;
                Current = current;
                RecordedReturn = recordedReturn;
                Parameters = parameters;
            }
        }

        public FieldReference? MoArray { get; }

        public FieldReference[] Mos { get; }

        public FieldReference MethodContext { get; }

        public FieldReference State { get; }

        public FieldReference Current { get; }

        public FieldReference? RecordedReturn { get; }

        public FieldReference?[] Parameters { get; }
    }
}
