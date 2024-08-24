using Cecil.AspectN.Patterns;
using Cecil.AspectN.Patterns.Parsers;

namespace Cecil.AspectN.Matchers
{
    public class PropertyMatcher : IMatcher
    {
        private readonly string _pattern;
        private readonly PropertyPattern _propertyPattern;

        public PropertyMatcher(string pattern)
        {
            _pattern = pattern;
            _propertyPattern = PropertyPatternParser.Parse(pattern);
        }

        public string Pattern => _pattern;

        public bool IsMatch(MethodSignature signature)
        {
            return _propertyPattern.IsMatch(signature);
        }

        public override string ToString()
        {
            return $"property({Pattern})";
        }
    }
}
