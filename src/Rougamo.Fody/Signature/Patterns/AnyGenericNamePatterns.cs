using System.Collections.Generic;

namespace Rougamo.Fody.Signature.Patterns
{
    public class AnyGenericNamePatterns : IGenericNamePatterns
    {
        public CollectionCount Count => CollectionCount.ANY;

        public void ExtractGenerics(List<GenericParameterTypePattern> list) { }

        public bool IsMatch(GenericSignature[] signatures) => true;
    }
}
