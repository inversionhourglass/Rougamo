using Mono.Cecil;

namespace Rougamo.Fody.Signature
{
    public class SimpleTypeSignature : TypeSignature
    {
        public SimpleTypeSignature(string name, TypeSignature? declaringType, TypeReference? reference) : base(name, declaringType, TypeCategory.Simple, reference)
        {
        }
    }
}
