using System;

namespace Rougamo.Fody.Signature.Patterns
{
    public class GenericParameterTypePattern : TypePattern
    {
        public GenericParameterTypePattern(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override GenericNamePattern ExtractNamePattern()
        {
            throw new NotImplementedException();
        }

        public override bool IsMatch(TypeSignature signature)
        {
            throw new NotImplementedException();
        }
    }
}
