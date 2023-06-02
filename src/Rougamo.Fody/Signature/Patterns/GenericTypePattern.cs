using Rougamo.Fody.Signature.Tokens;
using System;

namespace Rougamo.Fody.Signature.Patterns
{
    public class GenericTypePattern : TypePattern
    {
        public GenericTypePattern(TokenSource tokens, TypePattern[] genericArguments)
        {
            Tokens = tokens;
            GenericArguments = genericArguments;
        }

        public TokenSource Tokens { get; }

        public TypePattern[] GenericArguments { get; }

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
