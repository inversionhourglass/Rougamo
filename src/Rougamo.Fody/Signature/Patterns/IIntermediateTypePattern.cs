using System.Collections.Generic;

namespace Rougamo.Fody.Signature.Patterns
{
    public interface IIntermediateTypePattern : ITypePattern
    {
        GenericNamePattern SeparateOutMethod();

        void Compile(List<GenericParameterTypePattern> genericParameters, bool genericIn);
    }
}
