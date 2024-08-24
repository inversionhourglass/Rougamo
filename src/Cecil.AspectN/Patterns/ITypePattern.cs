namespace Cecil.AspectN.Patterns
{
    public interface ITypePattern
    {
        bool IsAny { get; }

        bool IsVoid { get; }

        bool AssignableMatch { get; }

        bool IsMatch(TypeSignature signature);
    }
}
