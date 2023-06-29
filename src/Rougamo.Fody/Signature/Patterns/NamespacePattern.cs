using Rougamo.Fody.Signature.Tokens;
using System;
using System.Collections.Generic;

namespace Rougamo.Fody.Signature.Patterns
{
    public class NamespacePattern : INamespacePattern
    {
        private readonly int _ellipsisCount;

        public NamespacePattern(TokenSource tokens)
        {
            Token? token;
            var pattern = string.Empty;
            var patterns = new List<string>();
            while ((token = tokens.Read()) != null)
            {
                if (token.IsDot())
                {
                    if (pattern.Length != 0)
                    {
                        patterns.Add(pattern);
                        pattern = string.Empty;
                    }
                }
                else if (token.IsEllipsis())
                {
                    if (pattern.Length != 0)
                    {
                        patterns.Add(pattern);
                        pattern = string.Empty;
                    }
                    _ellipsisCount++;
                    patterns.Add(Token.ELLIPSIS);
                }
                else
                {
                    pattern += token.ToString();
                }
            }
            if (pattern.Length != 0) patterns.Add(pattern);
            Patterns = patterns.ToArray();
        }

        public string[] Patterns { get; }

        public bool IsMatch(string @namespace)
        {
            var nss = @namespace.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (_ellipsisCount == 0 && nss.Length != Patterns.Length) return false;
            if (Patterns.Length == 2 && Patterns[0] == "*" && Patterns[1] == Token.ELLIPSIS) return true;

            int i = 0, j = 0;
            int iEllipsis = -1, jEllipsis = -1;

            while (i < nss.Length)
            {
                if (j < Patterns.Length && WildcardMatcher.IsMatch(Patterns[j], nss[i]))
                {
                    i++;
                    j++;
                }
                else if (j < Patterns.Length && Patterns[j] == Token.ELLIPSIS)
                {
                    iEllipsis = i;
                    jEllipsis = j + 1;
                    j++;
                }
                else if (iEllipsis >= 0)
                {
                    i = ++iEllipsis;
                    j = jEllipsis;
                }
                else
                {
                    return false;
                }
            }

            while (j < Patterns.Length && Patterns[j] == Token.ELLIPSIS)
            {
                j++;
            }

            return j == Patterns.Length;
        }
    }
}
