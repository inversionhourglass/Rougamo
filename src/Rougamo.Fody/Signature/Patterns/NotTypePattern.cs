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

        public bool IsAny => InnerPattern.IsAny;

        public bool IsVoid => false;

        public bool AssignableMatch => InnerPattern.AssignableMatch;

        public GenericNamePattern SeparateOutMethod()
        {
            return InnerPattern.SeparateOutMethod();
        }

        public DeclaringTypeMethodPattern ToDeclaringTypeMethod(params string[] methodImplicitPrefixes)
        {
            var method = SeparateOutMethod();
            method.ImplicitPrefixes = methodImplicitPrefixes;
            return new NotDeclaringTypeMethodPattern(InnerPattern, method);
        }

        public void Compile(List<GenericParameterTypePattern> genericParameters, bool genericIn)
        {
            InnerPattern.Compile(genericParameters, genericIn);
        }

        public bool IsMatch(TypeSignature signature)
        {
            return InnerPattern.IsAny || !InnerPattern.IsMatch(signature);
        }
    }
}
