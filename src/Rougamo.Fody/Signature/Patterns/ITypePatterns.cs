namespace Rougamo.Fody.Signature.Patterns
{
    public interface ITypePatterns
    {
        CollectionCount Count { get; }

        bool IsMatch(TypeSignature[] signature);
    }
}
