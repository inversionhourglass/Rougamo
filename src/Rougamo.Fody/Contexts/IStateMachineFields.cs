using Mono.Cecil;

namespace Rougamo.Fody.Contexts
{
    internal interface IStateMachineFields
    {
        FieldDefinition[] Mos { get; }

        FieldDefinition MethodContext { get; }

        FieldDefinition State { get; }

        FieldDefinition?[] Parameters { get; }

        FieldDefinition? DeclaringThis { get; set; }

        void SetParameter(int index, FieldDefinition fieldDef);
    }
}
