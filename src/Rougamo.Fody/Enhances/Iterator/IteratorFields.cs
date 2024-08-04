using Mono.Cecil;

namespace Rougamo.Fody.Enhances.Iterator
{
    internal class IteratorFields(
        FieldDefinition? moArray, FieldDefinition[] mos,
        FieldDefinition methodContext,
        FieldDefinition state, FieldDefinition current,
        FieldDefinition initialThreadId, FieldDefinition? recordedReturn,
        FieldDefinition? declaringThis, FieldDefinition iterator,
        FieldDefinition?[] transitParameters, FieldDefinition?[] parameters) : IIteratorFields
    {
        public FieldDefinition? MoArray { get; } = moArray;

        public FieldDefinition[] Mos { get; } = mos;

        public FieldDefinition MethodContext { get; } = methodContext!;

        public FieldDefinition State { get; } = state!;

        public FieldDefinition Current { get; } = current!;

        public FieldDefinition InitialThreadId { get; } = initialThreadId!;

        public FieldDefinition? RecordedReturn { get; } = recordedReturn;

        public FieldDefinition?[] TransitParameters { get; set; } = transitParameters;

        public FieldDefinition?[] Parameters { get; } = parameters;

        public FieldDefinition? DeclaringThis { get; set; } = declaringThis;

        public FieldDefinition Iterator { get; set; } = iterator!;

        public void SetParameter(int index, FieldDefinition fieldDef)
        {
            Parameters[index] = fieldDef;
        }
    }
}
