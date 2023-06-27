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

        public bool IsAny => false;

        public bool AssignableMatch => throw new NotImplementedException();

        public bool IsMatch(TypeSignature signature)
        {
            return signature is GenericParameterTypeSignature gpts && gpts.VirtualName == VirtualName;
        }

        public void Compile(List<GenericParameterTypePattern> genericParameters, bool genericIn)
        {

        }

        public GenericNamePattern SeparateOutMethod()
        {
            throw new NotImplementedException();
        }

        public DeclaringTypeMethodPattern ToDeclaringTypeMethod()
        {
            throw new NotImplementedException();
        }
    }
}
