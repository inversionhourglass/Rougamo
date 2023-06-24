namespace Rougamo.Fody.Signature.Patterns
{
    public class OnePlusTypePatterns : ITypePatterns
    {
        public CollectionCount Count => CollectionCount.OnePlus;

        public bool IsMatch(TypeSignature[] signature) => signature.Length == Count;
    }
}
