using System.Collections.Generic;

namespace Rougamo.Fody.Signature.Patterns
{
    public class AnyTypePattern : ITypePattern, IIntermediateTypePattern
    {
        public bool AssignableMatch => false;

        public void Compile(List<GenericParameterTypePattern> genericParameters, bool genericIn)
        {
            
        }

        public GenericNamePattern ExtractNamePattern()
        {
            throw new System.NotImplementedException();
        }

        public bool IsMatch(TypeSignature signature) => true;
    }
}
