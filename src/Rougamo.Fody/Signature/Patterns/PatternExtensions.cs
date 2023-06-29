namespace Rougamo.Fody.Signature.Patterns
{
    public static class PatternExtensions
    {
        public static bool IsSystem(this INamespacePattern @namespace) => @namespace is SystemNamespacePattern || @namespace is NamespacePattern np && np.Patterns.Length == 1 && np.Patterns[0] == "System";

        public static bool IsSimpleName(this IGenericNamePatterns genericNamePatterns, string simpleName)
        {
            if (genericNamePatterns is SingleNonGenericNamePattern sngnp && sngnp.Name == simpleName) return true;
            if (genericNamePatterns is not GenericNamePatterns gnps || gnps.Patterns.Length != 1) return false;
            var pattern = gnps.Patterns[0];
            return pattern.Name == simpleName && pattern.GenericPatterns.Count == 0;
        }
    }
}
