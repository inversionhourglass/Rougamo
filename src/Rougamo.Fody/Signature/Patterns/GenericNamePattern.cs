using System;

namespace Rougamo.Fody.Signature.Patterns
{
    public class GenericNamePattern : NamePattern
    {
        public GenericNamePattern(string name, TypePattern[]? genericParameters)
        {
            Name = name;
            GenericParameters = genericParameters;
        }

        public string Name { get; }

        public TypePattern[]? GenericParameters { get; }

        public override bool IsMatch(string name)
        {
            throw new NotImplementedException();
        }
    }
}
