using System.Collections.Generic;

namespace Rougamo.Fody.Signature.Patterns
{
    public interface IGenericNamePatterns
    {
        CollectionCount Count { get; }

        void ExtractGenerics(List<GenericParameterTypePattern> list);

        bool IsMatch(GenericSignature[] signatures);
    }
}
