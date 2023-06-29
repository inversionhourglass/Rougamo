using System;

namespace Rougamo.Fody.Signature.Matchers
{
    public class MatcherFactory
    {
        public static IMatcher Create(bool not, string method, string pattern)
        {
            var matcher = Create(method.Trim().ToLower(), pattern);
            return not ? new NotMatcher(matcher) : matcher;
        }

        private static IMatcher Create(string method, string pattern) => method switch
        {
            "regex" => new RegexMatcher(pattern),
            "execution" => new ExecutionMatcher(pattern),
            "method" => new MethodMatcher(pattern),
            "getter" => new GetterMatcher(pattern),
            "setter" => new SetterMatcher(pattern),
            "property" => new PropertyMatcher(pattern),
            _ => throw new ArgumentException($"unknow matcher type({method}) with pattern {pattern}")
        };
    }
}
