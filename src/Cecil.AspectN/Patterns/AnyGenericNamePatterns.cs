using System.Collections.Generic;

namespace Cecil.AspectN.Patterns
{
    public class AnyGenericNamePatterns : IGenericNamePatterns
    {
        public CollectionCount Count => CollectionCount.ANY;

        public void ExtractGenerics(List<GenericParameterTypePattern> list) { }

        public bool IsMatch(GenericSignature[] signatures) => true;
    }
}
