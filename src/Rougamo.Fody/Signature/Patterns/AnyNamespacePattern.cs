namespace Rougamo.Fody.Signature.Patterns
{
    public class AnyNamespacePattern : INamespacePattern
    {
        public bool IsMatch(string @namespace) => true;
    }
}
