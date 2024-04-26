using Mono.Cecil;

namespace Rougamo.Fody.Enhances
{
    internal interface IIteratorFields : IStateMachineFields
    {
        FieldReference Current { get; }

        FieldReference? RecordedReturn { get; }

        FieldReference?[] TransitParameters { get; set; }
    }
}
