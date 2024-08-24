using System.Collections.Generic;

namespace Cecil.AspectN.Patterns
{
    public interface IGenericNamePatterns
    {
        CollectionCount Count { get; }

        void ExtractGenerics(List<GenericParameterTypePattern> list);

        bool IsMatch(GenericSignature[] signatures);
    }
}
