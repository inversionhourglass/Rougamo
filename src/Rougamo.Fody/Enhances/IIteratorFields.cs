using Mono.Cecil;

namespace Rougamo.Fody.Contexts
{
    internal interface IIteratorFields : IStateMachineFields
    {
        FieldDefinition Current { get; }

        FieldDefinition? RecordedReturn { get; }

        FieldDefinition?[] TransitParameters { get; set; }
    }
}
