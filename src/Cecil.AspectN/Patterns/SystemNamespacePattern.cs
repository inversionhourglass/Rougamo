namespace Cecil.AspectN.Patterns
{
    public class SystemNamespacePattern : INamespacePattern
    {
        public bool IsMatch(string @namespace) => @namespace == "System";
    }
}
