namespace Rougamo.Fody.Signature.Patterns
{
    public interface ITypePattern
    {
        bool AssignableMatch { get; }

        bool IsMatch(TypeSignature signature);
    }
}
