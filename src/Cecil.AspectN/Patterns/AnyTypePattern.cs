using System.Collections.Generic;

namespace Cecil.AspectN.Patterns
{
    public class AnyTypePattern : ITypePattern, IIntermediateTypePattern
    {
        public bool IsAny => true;

        public bool IsVoid => false;

        public bool AssignableMatch => false;

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

        public bool IsMatch(TypeSignature signature) => true;
    }
}
