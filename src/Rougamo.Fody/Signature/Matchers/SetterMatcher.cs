using System;

namespace Rougamo.Fody.Signature.Matchers
{
    public class SetterMatcher : IMatcher
    {
        private readonly string _pattern;

        public SetterMatcher(string pattern)
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
            return $"setter({_pattern})";
        }
    }
}
