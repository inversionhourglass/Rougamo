namespace Cecil.AspectN.Patterns
{
    public class AnyNamespacePattern : INamespacePattern
    {
        public bool IsMatch(string @namespace) => true;
    }
}
