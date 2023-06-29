using Rougamo.Fody.Signature.Patterns.Parsers;
using Rougamo.Fody.Signature.Patterns;

namespace Rougamo.Fody.Signature.Matchers
{
    public class MethodMatcher : IMatcher
    {
        private readonly string _pattern;
        private readonly MethodPattern _methodPattern;

        public MethodMatcher(string pattern)
        {
            _pattern = pattern;
            _methodPattern = MethodPatternParser.Parse(pattern);
        }

        public string Pattern => _pattern;

        public bool IsMatch(MethodSignature signature)
        {
            return _methodPattern.IsMatch(signature);
        }

        public override string ToString()
        {
            return $"method({_pattern})";
        }
    }
}
