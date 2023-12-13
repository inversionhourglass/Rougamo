using Rougamo.Fody.Signature.Patterns.Parsers;
using Rougamo.Fody.Signature.Patterns;

namespace Rougamo.Fody.Signature.Matchers
{
    public class AttributeMatcher : IMatcher
    {
        private readonly string _pattern;
        private readonly AttributePattern _attributePattern;

        public AttributeMatcher(string pattern)
        {
            _pattern = pattern;
            _attributePattern = AttributePatternParser.Parse(pattern);
        }

        public string Pattern => _pattern;

        public bool IsMatch(MethodSignature signature)
        {
            return _attributePattern.IsMatch(signature.Attributes);
        }

        public override string ToString()
        {
            return $"attr({Pattern})";
        }
    }
}
