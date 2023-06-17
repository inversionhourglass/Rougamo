using System.Collections.Generic;

namespace Rougamo.Fody.Signature.Patterns
{
    public class NotTypePattern : IIntermediateTypePattern
    {
        public NotTypePattern(IIntermediateTypePattern innerPattern)
        {
            InnerPattern = innerPattern;
        }

        public IIntermediateTypePattern InnerPattern { get; }

        public bool AssignableMatch => InnerPattern.AssignableMatch;

        public GenericNamePattern ExtractNamePattern()
        {
            return InnerPattern.ExtractNamePattern();
        }

        public void Compile(List<GenericParameterTypePattern> genericParameters, bool genericIn)
        {
            InnerPattern.Compile(genericParameters, genericIn);
        }

        public bool IsMatch(TypeSignature signature)
        {
            return !InnerPattern.IsMatch(signature);
        }
    }
}
