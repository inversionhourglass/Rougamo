namespace Rougamo.Fody.Signature.Patterns
{
    public class AnyTypePatterns : ITypePatterns
    {
        public CollectionCount Count => CollectionCount.ANY;

        public bool IsMatch(TypeSignature[] signature) => true;
    }
}
