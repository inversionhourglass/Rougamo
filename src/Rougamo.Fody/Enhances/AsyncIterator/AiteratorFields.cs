using Mono.Cecil;

namespace Rougamo.Fody.Enhances.AsyncIterator
{
    internal class AiteratorFields(
        FieldDefinition? moArray, FieldDefinition[] mos,
        FieldDefinition methodContext,
        FieldDefinition state, FieldDefinition current,
        FieldDefinition initialThreadId, FieldDefinition disposed,
        FieldDefinition builder, FieldDefinition promise,
        FieldDefinition? recordedReturn, FieldDefinition? declaringThis,
        FieldDefinition iterator, FieldDefinition awaiter, FieldDefinition moAwaiter,
        FieldDefinition?[] transitParameters, FieldDefinition?[] parameters) : IIteratorFields
    {
        public FieldDefinition? MoArray { get; } = moArray;

        public FieldDefinition[] Mos { get; } = mos;

        public FieldDefinition MethodContext { get; } = methodContext!;

        public FieldDefinition State { get; } = state!;

        public FieldDefinition Current { get; } = current!;

        public FieldDefinition InitialThreadId { get; } = initialThreadId!;

        public FieldDefinition Disposed { get; } = disposed!;

        public FieldDefinition Builder { get; } = builder!;

        public FieldDefinition Promise { get; } = promise!;

        public FieldDefinition? RecordedReturn { get; } = recordedReturn;

        public FieldDefinition?[] TransitParameters { get; set; } = transitParameters;

        public FieldDefinition?[] Parameters { get; } = parameters;

        public FieldDefinition? DeclaringThis { get; set; } = declaringThis;

        public FieldDefinition Awaiter { get; set; } = awaiter!;

        public FieldDefinition Iterator { get; set; } = iterator!;

        public FieldDefinition MoAwaiter { get; } = moAwaiter!;

        public void SetParameter(int index, FieldDefinition fieldDef)
        {
            Parameters[index] = fieldDef;
        }
    }
}
