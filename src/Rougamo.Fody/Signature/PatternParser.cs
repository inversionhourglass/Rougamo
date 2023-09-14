using Rougamo.Fody.Signature.Matchers;
using System;

namespace Rougamo.Fody.Signature
{
    public class PatternParser
    {
        public static IMatcher Parse(string pattern)
        {
            var index = 0;
            return Parse(pattern, ref index);
        }

        private static IMatcher Parse(string pattern, ref int index)
        {
            var matcher = ParseSingle(pattern, ref index);
            var connector = ParseConnector(pattern, ref index);
            if (connector == Connector.And)
            {
                matcher = new AndMatcher(matcher, ParseNotOr(pattern, ref index, out connector));
            }
            if (connector == Connector.Or)
            {
                matcher = new OrMatcher(matcher, Parse(pattern, ref index));
            }

            return matcher;
        }

        private static IMatcher ParseNotOr(string pattern, ref int index, out Connector connector)
        {
            var matcher = ParseSingle(pattern, ref index);
            connector = ParseConnector(pattern, ref index);
            if (connector == Connector.And)
            {
                matcher = new AndMatcher(matcher, ParseNotOr(pattern, ref index, out connector));
            }

            return matcher;
        }

        private static IMatcher ParseSingle(string pattern, ref int index)
        {
            var not = ParseNot(pattern, ref index);
            var method = ParseMethod(pattern, ref index);
            var startSymbol = pattern[index++];
            if (startSymbol != '(') throw new ArgumentException($"Expect pattern body start with '(', but got '{startSymbol}'");
            var matchPattern = ParseMatchPattern(pattern, ref index);
            var endSymbol = pattern[index++];
            if (endSymbol != ')') throw new ArgumentException($"Expect pattern body end with ')', but got '{endSymbol}'");

            return MatcherFactory.Create(not, method, matchPattern);
        }

        private static bool ParseNot(string pattern, ref int index)
        {
            var stash = index;
            while (index < pattern.Length)
            {
                var ch = pattern[index++];
                if (ch.IsWhiteSpace()) continue;
                if (ch == '!') return true;
                break;
            }
            index = stash;
            return false;
        }

        private static Connector ParseConnector(string pattern, ref int index)
        {
            while (index < pattern.Length)
            {
                var ch = pattern[index++];
                if (ch.IsWhiteSpace()) continue;
                if (ch == '&')
                {
                    if (index < pattern.Length)
                    {
                        var next = pattern[index++];
                        if (next == '&') return Connector.And;
                        throw new ArgumentException($"Unexpected AND connector symbol '{next}' at index {index} of {pattern}");
                    }
                    return Connector.Eof;
                }
                if (ch == '|')
                {
                    if (index < pattern.Length)
                    {
                        var next = pattern[index++];
                        if (next == '|') return Connector.Or;
                        throw new ArgumentException($"Unexpected OR connector symbol '{next}' at index {index} of {pattern}");
                    }
                    return Connector.Eof;
                }
                throw new ArgumentException($"Unexpected connector symbol '{ch}' at index {index} of {pattern}");
            }
            return Connector.Eof;
        }

        private static string ParseMethod(string pattern, ref int index)
        {
            var start = index;
            while (index < pattern.Length)
            {
                var ch = pattern[index++];
                if (ch.IsWhiteSpace()) continue;
                if (ch < 'A' || ch > 'Z' && ch < 'a' || ch > 'z')
                {
                    index--;
                    break;
                }
            }

            if (start == index) throw new ArgumentException($"Unknow pattern method ({pattern[index]}) at index {index} of {pattern}");
            return pattern.Substring(start, index - start);
        }

        private static string ParseMatchPattern(string pattern, ref int index)
        {
            char ch;
            var start = index;
            var groups = 0;
            var escape = false;
            var inRange = false;
            while (index < pattern.Length)
            {
                ch = pattern[index];
                switch (ch)
                {
                    case '\\':
                        escape ^= true;
                        break;
                    case '[':
                        inRange = !escape;
                        escape = false;
                        break;
                    case ']':
                        inRange = inRange && escape;
                        escape = false;
                        break;
                    case '(':
                        if (!escape && !inRange) groups++;
                        escape = false;
                        break;
                    case ')':
                        if (!escape && !inRange)
                        {
                            if (groups == 0) return pattern.Substring(start, index - start);
                            groups--;
                        }
                        escape = false;
                        break;
                    default:
                        escape = false;
                        break;
                }
                index++;
            }
            throw new ArgumentException($"Unable parse regex token source from index {start} to the end of pattern({pattern})");
        }

        enum Connector
        {
            Eof,
            And,
            Or,
        }
    }
}
