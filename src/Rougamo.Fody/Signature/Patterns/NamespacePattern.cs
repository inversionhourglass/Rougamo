using Rougamo.Fody.Signature.Tokens;
using System;
using System.Collections.Generic;

namespace Rougamo.Fody.Signature.Patterns
{
    public class NamespacePattern : INamespacePattern
    {
        private readonly string[] _patterns;
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
            _patterns = patterns.ToArray();
        }

        public bool IsMatch(string @namespace)
        {
            var nss = @namespace.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (_ellipsisCount == 0 && nss.Length != _patterns.Length) return false;
            if (_patterns.Length == 2 && _patterns[0] == "*" && _patterns[1] == Token.ELLIPSIS) return true;

            int i = 0, j = 0;
            int iEllipsis = -1, jEllipsis = -1;

            while (i < nss.Length)
            {
                if (j < _patterns.Length && WildcardMatcher.IsMatch(_patterns[j], nss[i]))
                {
                    i++;
                    j++;
                }
                else if (j < _patterns.Length && _patterns[j] == Token.ELLIPSIS)
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

            while (j < _patterns.Length && _patterns[j] == Token.ELLIPSIS)
            {
                j++;
            }

            return j == _patterns.Length;
        }
    }
}
