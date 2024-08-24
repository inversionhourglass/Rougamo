using System.Collections.Generic;

namespace Cecil.AspectN.Patterns
{
    public class NullTypePattern : IIntermediateTypePattern, ITypePattern
    {
        public bool IsAny => false;

        public bool IsVoid => false;

        public bool AssignableMatch => false;

        public bool IsMatch(TypeSignature signature) => false;

        public void Compile(List<GenericParameterTypePattern> genericParameters, bool genericIn)
        {
            
        }

        public GenericNamePattern SeparateOutMethod()
        {
            throw new System.NotImplementedException();
        }

        public DeclaringTypeMethodPattern ToDeclaringTypeMethod(params string[] methodImplicitPrefixes)
        {
            throw new System.NotImplementedException();
        }
    }
}
