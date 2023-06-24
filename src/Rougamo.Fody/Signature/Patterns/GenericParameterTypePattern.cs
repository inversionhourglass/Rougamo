using System;
using System.Collections.Generic;

namespace Rougamo.Fody.Signature.Patterns
{
    public class GenericParameterTypePattern : CompiledTypePattern, ITypePattern, IIntermediateTypePattern
    {
        public GenericParameterTypePattern(string name) : base(new AnyNamespacePattern(), new AnyGenericNamePatterns(), false)
        {
            Name = name;
        }

        public string Name { get; }

        public string? VirtualName { get; set; }

        public override bool IsMatch(TypeSignature signature)
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
    }
}
