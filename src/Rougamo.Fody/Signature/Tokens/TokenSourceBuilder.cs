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
            }
            if (index != pattern.Length) throw new ArgumentException($"Cannot parse pattern({pattern}) to tokens, stopped at index {index}");

            return new TokenSource(tokens.ToArray());
        }

        private static Token? Build(string pattern, ref int index)
        {
            while (index < pattern.Length)
            {
                var ch = pattern[index++];
                switch (ch)
                {
                    case ' ':
                    case '　':
                    case '\t':
                    case '\r':
                    case '\n':
                        while (index < pattern.Length && pattern[index].IsWhiteSpace()) index++;
                        return Token.Space;
                    case '*':
                    case '+':
                    case ',':
                    case '!':
                    case '(':
                    case ')':
                    case '<':
                    case '>':
                    case '[':
                    case ']':
                        return new Token(ch);
                    case '.':
                        if (index < pattern.Length)
                        {
                            var next = pattern[index];
                            if (next == '.')
                            {
                                index++;
                                return new Token("..");
                            }
                        }
                        return new Token(ch);
                    case '&':
                    case '|':
                        if(index < pattern.Length)
                        {
                            var next = pattern[index++];
                            if (next != ch) throw new ArgumentException($"Unexpected connector symbol '{next}' at index {index - 1} of {pattern}");
                            return new Token(new string(ch, 2));
                        }
                        throw new ArgumentException($"Unexpected connector symbol '{ch}' at index {index - 1} of {pattern}");
                    default:
                        index--;
                        return BuildNameIdentifier(pattern, ref index);
                }
            }

            return null;
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
