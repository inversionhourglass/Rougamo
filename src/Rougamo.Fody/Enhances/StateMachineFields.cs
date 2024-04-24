using Mono.Cecil;

namespace Rougamo.Fody.Enhances
{
    internal abstract class StateMachineFields
    {
        protected StateMachineFields(TypeDefinition stateMachineTypeDef)
        {
            if (stateMachineTypeDef.HasGenericParameters)
            {
                // public async Task<MyClass<T>> Mt<T>()
                DeclaringTypeRef = new GenericInstanceType(stateMachineTypeDef);
                foreach (var parameter in stateMachineTypeDef.GenericParameters)
                {
                    DeclaringTypeRef.GenericArguments.Add(parameter);
                }
            }
        }

        public GenericInstanceType? DeclaringTypeRef { get; }

        protected FieldReference? MakeReference(FieldDefinition? fieldDef)
        {
            if (DeclaringTypeRef == null || fieldDef == null) return fieldDef;

            return new FieldReference(fieldDef.Name, fieldDef.FieldType, DeclaringTypeRef);
        }
    }
}
