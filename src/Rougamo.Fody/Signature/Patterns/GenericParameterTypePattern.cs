using System;
using System.Collections.Generic;

namespace Rougamo.Fody.Signature.Patterns
{
    public class GenericParameterTypePattern : ITypePattern, IIntermediateTypePattern
    {
        public GenericParameterTypePattern(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public string? VirtualName { get; set; }

        public bool AssignableMatch => false;

        public bool IsMatch(TypeSignature signature)
        {
            return signature is GenericParameterTypeSignature gpts && gpts.VirtualName == VirtualName;
        }

        public void Compile(List<GenericParameterTypePattern> genericParameters, bool genericIn)
        {

        }

        public GenericNamePattern ExtractNamePattern()
        {
            throw new NotImplementedException();
        }
    }
}
