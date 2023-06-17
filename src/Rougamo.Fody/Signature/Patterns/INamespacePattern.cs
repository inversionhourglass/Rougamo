namespace Rougamo.Fody.Signature.Patterns
{
    public interface INamespacePattern
    {
        bool IsMatch(string @namespace);
    }
}
