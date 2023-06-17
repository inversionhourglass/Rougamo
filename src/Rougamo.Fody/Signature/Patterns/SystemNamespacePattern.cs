namespace Rougamo.Fody.Signature.Patterns
{
    public class SystemNamespacePattern : INamespacePattern
    {
        public bool IsMatch(string @namespace) => @namespace == "System";
    }
}
