using Mono.Cecil;

namespace Rougamo.Fody.Enhances.Async
{
    internal class AsyncFields : IStateMachineFields
    {
        public AsyncFields(
            FieldDefinition? moArray, FieldDefinition[] mos,
            FieldDefinition methodContext, FieldDefinition state,
            FieldDefinition builder, FieldDefinition? declaringThis,
            FieldDefinition awaiter, FieldDefinition? moAwaiter,
            FieldDefinition? result, FieldDefinition?[] parameters)
        {
            MoArray = moArray;
            Mos = mos;
            MethodContext = methodContext;
            State = state;
            Builder = builder;
            Parameters = parameters;
            DeclaringThis = declaringThis;
            Awaiter = awaiter;
            MoAwaiter = moAwaiter ?? Awaiter;
            Result = result;
        }

        public FieldDefinition? MoArray { get; }

        public FieldDefinition[] Mos { get; }

        public FieldDefinition MethodContext { get; }

        public FieldDefinition State { get; }

        public FieldDefinition Builder { get; }

        public FieldDefinition?[] Parameters { get; }

        public FieldDefinition? DeclaringThis { get; set; }

        public FieldDefinition Awaiter { get; set; }

        public FieldDefinition MoAwaiter { get; set; }

        public FieldDefinition? Result { get; set; }

        public void SetParameter(int index, FieldDefinition fieldDef)
        {
            Parameters[index] = fieldDef;
        }
    }
}
