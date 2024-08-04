using Mono.Cecil;

namespace Rougamo.Fody.Enhances
{
    internal interface IStateMachineFields
    {
        FieldDefinition? MoArray { get; }

        FieldDefinition[] Mos { get; }

        FieldDefinition MethodContext { get; }

        FieldDefinition State { get; }

        FieldDefinition?[] Parameters { get; }

        FieldDefinition? DeclaringThis { get; set; }

        void SetParameter(int index, FieldDefinition fieldDef);
    }
}
