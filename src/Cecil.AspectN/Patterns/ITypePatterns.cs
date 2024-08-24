namespace Cecil.AspectN.Patterns
{
    public interface ITypePatterns
    {
        CollectionCount Count { get; }

        bool IsMatch(TypeSignature[] signature);
    }
}
