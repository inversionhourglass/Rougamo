using Mono.Cecil;

namespace Rougamo.Fody.Signature
{
    public class GenericParameterTypeSignature : TypeSignature
    {
        public GenericParameterTypeSignature(string name, TypeReference reference) : base(name, null, TypeCategory.GenericParameter, reference)
        {
        }

        public string? SortName { get; set; }

        protected override string FormatName => SortName ?? Name;
    }
}
