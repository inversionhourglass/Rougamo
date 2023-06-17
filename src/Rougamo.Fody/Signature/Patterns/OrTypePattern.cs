using System.Collections.Generic;

namespace Rougamo.Fody.Signature.Patterns
{
    public class OrTypePattern : IIntermediateTypePattern
    {
        public OrTypePattern(IIntermediateTypePattern left, IIntermediateTypePattern right)
        {
            Left = left;
            Right = right;
        }

        public IIntermediateTypePattern Left { get; }

        public IIntermediateTypePattern Right { get; }

        public bool AssignableMatch => Left.AssignableMatch || Right.AssignableMatch;

        public GenericNamePattern ExtractNamePattern()
        {
            return Right.ExtractNamePattern();
        }

        public void Compile(List<GenericParameterTypePattern> genericParameters, bool genericIn)
        {
            Left.Compile(genericParameters, genericIn);
            Right.Compile(genericParameters, genericIn);
        }

        public bool IsMatch(TypeSignature signature)
        {
            return Left.IsMatch(signature) || Right.IsMatch(signature);
        }
    }
}
