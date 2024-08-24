using System.Text.RegularExpressions;

namespace Cecil.AspectN.Matchers
{
    public class RegexMatcher : IMatcher
    {
        private readonly Regex _regex;

        public RegexMatcher(string pattern)
        {
            Pattern = pattern;
            _regex = new Regex(pattern);
        }

        public string Pattern { get; }

        public bool IsMatch(MethodSignature signature)
        {
            return _regex.IsMatch(signature.ToString());
        }

        public override string ToString()
        {
            return $"regex({Pattern})";
        }
    }
}
