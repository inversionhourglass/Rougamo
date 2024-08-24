using Cecil.AspectN.Patterns;
using Cecil.AspectN.Patterns.Parsers;

namespace Cecil.AspectN.Matchers
{
    public class SetterMatcher : IMatcher
    {
        private readonly string _pattern;
        private readonly SetterPattern _setterPattern;

        public SetterMatcher(string pattern)
        {
            _pattern = pattern;
            _setterPattern = SetterPatternParser.Parse(pattern);
        }

        public string Pattern => _pattern;

        public bool IsMatch(MethodSignature signature)
        {
            return _setterPattern.IsMatch(signature);
        }

        public override string ToString()
        {
            return $"setter({_pattern})";
        }
    }
}
