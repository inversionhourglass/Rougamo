namespace Rougamo.Fody.Signature.Patterns
{
    public class NoneTypePatterns : ITypePatterns
    {
        public CollectionCount Count => CollectionCount.ZERO;

        public bool IsMatch(TypeSignature[] signature)
        {
            return signature.Length == 0;
        }
    }
}
