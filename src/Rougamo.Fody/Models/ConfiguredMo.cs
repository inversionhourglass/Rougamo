using Mono.Cecil;

namespace Rougamo.Fody.Models
{
    internal class ConfiguredMo(TypeReference typeRef, string? pattern) : TypeReferenceMo(typeRef, MoFrom.Assembly)
    {
        protected override string? ExtractPattern() => pattern ?? base.ExtractPattern();
    }
}
