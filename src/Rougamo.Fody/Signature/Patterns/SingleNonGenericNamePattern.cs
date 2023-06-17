using System.Collections.Generic;

namespace Rougamo.Fody.Signature.Patterns
{
    public class SingleNonGenericNamePattern : IGenericNamePatterns
    {
        public SingleNonGenericNamePattern(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public CollectionCount Count => CollectionCount.Single;

        public void ExtractGenerics(List<GenericParameterTypePattern> list) { }

        public bool IsMatch(GenericSignature[] signatures)
        {
            if (signatures.Length != 1) return false;
            var signature = signatures[0];
            return signature.Generics.Length == 0 && WildcardMatcher.IsMatch(Name, signature.Name);
        }
    }
}
