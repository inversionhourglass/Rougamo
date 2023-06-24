using Rougamo.Fody.Signature.Patterns;
using Rougamo.Fody.Signature.Patterns.Parsers;

namespace Rougamo.Fody.Signature.Matchers
{
    public class ExecutionMatcher : IMatcher
    {
        private readonly string _pattern;
        private readonly ExecutionPattern _executionPattern;

        public ExecutionMatcher(string pattern)
        {
            _pattern = pattern;
            _executionPattern = ExecutionPatternParser.Parse(pattern);
        }

        public string Pattern => _pattern;

        public bool IsMatch(MethodSignature signature)
        {
            return _executionPattern.IsMatch(signature);
        }

        public override string ToString()
        {
            return $"execution({_pattern})";
        }
    }
}
