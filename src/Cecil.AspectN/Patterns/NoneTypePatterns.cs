namespace Cecil.AspectN.Patterns
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
