using System;

namespace Rougamo.Fody.Signature.Matchers
{
    public class GetterMatcher : IMatcher
    {
        private readonly string _pattern;

        public GetterMatcher(string pattern)
        {
            _pattern = pattern;
        }

        public string Pattern => _pattern;

        public bool IsMatch(MethodSignature signature)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"getter({_pattern})";
        }
    }
}
