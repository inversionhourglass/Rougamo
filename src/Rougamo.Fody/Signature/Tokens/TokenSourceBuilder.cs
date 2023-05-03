using System;
using System.Collections.Generic;

namespace Rougamo.Fody.Signature.Tokens
{
    public class TokenSourceBuilder
    {
        public static TokenSource Build(string pattern)
        {
            var tokens = new List<Token>();
            var index = 0;
            while (true)
            {
                var token = Build(pattern, ref index);
                if (token == null) break;

                tokens.Add(token);
                if (tokens.Count == 1 && token.IsRegex())
                {
                    var regexToken = BuildRegex(pattern, ref index);
                    tokens.Add(new Token('('));
                    tokens.Add(regexToken);
                    tokens.Add(new Token(')'));
                }
            }
            if (index != pattern.Length) throw new ArgumentException($"Cannot parse pattern({pattern}) to tokens, stopped at index {index}");

            return new TokenSource(tokens.ToArray());
        }

        private static Token? Build(string pattern, ref int index)
        {
            while (index < pattern.Length)
            {
                var ch = pattern[index];
                switch (ch)
                {
                    case ' ':
                    case '　':
                    case '\t':
                    case '\r':
                    case '\n':
                        index++;
                        continue;
                    case '*':
                    case '+':
                    case ',':
                    case '!':
                    case '(':
                    case ')':
                    case '<':
                    case '>':
                        return new Token(ch);
                    default:
                        return BuildNameIdentifier(pattern, ref index);
                }
            }

            return null;
        }

        private static Token BuildRegex(string pattern, ref int index)
        {
            var ch = pattern[index];
            if (ch != '(') throw new ArgumentException($"Regex token source expected '(' but actualy {ch}, at index {index} of pattern({pattern})");
            var start = ++index;
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
                            if (groups == 0) return new Token(pattern.Substring(start, index++ - start));
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

        private static Token BuildNameIdentifier(string pattern, ref int index)
        {
            var start = index;
            while (index < pattern.Length)
            {
                var ch = pattern[index];
                if (ch != '_' && (ch < '0' || ch > '9' && ch < 'A' || ch > 'Z' && ch < 'a' || ch > 'z')) break;

                index++;
            }

            if (start == index) throw new ArgumentException($"unknow token ({pattern[index]}) at index {index} of {pattern}");
            return new Token(pattern.Substring(start, index - start));
        }
    }
}
