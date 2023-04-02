using Mono.Cecil;

namespace Rougamo.Fody.Enhances
{
    internal interface IStateMachineFields
    {
        FieldReference? MoArray { get; }

        FieldReference[] Mos { get; }

        FieldReference MethodContext { get; }

        FieldReference State { get; }

        FieldReference?[] Parameters { get; }
    }
}
