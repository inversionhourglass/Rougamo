using Cecil.AspectN.Patterns;
using Cecil.AspectN.Patterns.Parsers;

namespace Cecil.AspectN.Matchers
{
    public class GetterMatcher : IMatcher
    {
        private readonly string _pattern;
        private readonly GetterPattern _getterPattern;

        public GetterMatcher(string pattern)
        {
            _pattern = pattern;
            _getterPattern = GetterPatternParser.Parse(pattern);
        }

        public string Pattern => _pattern;

        public bool IsMatch(MethodSignature signature)
        {
            return _getterPattern.IsMatch(signature);
        }

        public override string ToString()
        {
            return $"getter({_pattern})";
        }
    }
}
