using Mono.Cecil;

namespace Rougamo.Fody.Enhances
{
    internal interface IIteratorFields : IStateMachineFields
    {
        FieldDefinition Current { get; }

        FieldDefinition? RecordedReturn { get; }

        FieldDefinition?[] TransitParameters { get; set; }
    }
}
