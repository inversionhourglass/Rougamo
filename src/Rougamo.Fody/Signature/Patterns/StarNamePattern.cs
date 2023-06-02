using Rougamo.Fody.Signature.Tokens;
using System.Linq;

namespace Rougamo.Fody.Signature.Patterns
{
    public class StarNamePattern : NamePattern
    {
        private readonly string _pattern;
        private readonly int _nonStarCount;

        public StarNamePattern(string pattern)
        {
            _pattern = pattern;
            _nonStarCount = _pattern.Count(x => x != Token.STAR);
        }

        public override bool IsMatch(string name)
        {
            if (_nonStarCount > name.Length) return false;

            if (_nonStarCount == _pattern.Length) return _pattern == name;

            if (_nonStarCount == _pattern.Length - 1)
            {
                if (_pattern[0] == '*') return name.EndsWith(_pattern.Substring(1));
                if (_pattern[_pattern.Length - 1] == '*') return name.StartsWith(_pattern.Substring(0, _pattern.Length - 1));
            }

            var dp = new bool[name.Length];
            dp[0] = name[0] == _pattern[0] || _pattern[0] == '*';
            for (var i = 1; i < name.Length; i++)
            {
                dp[i] = _pattern[0] == '*';
            }
            for (var j = 1; j < _pattern.Length; j++)
            {
                var p = _pattern[j];
                var ifPreAny = false;
                var dpPre = dp[0];
                dp[0] = (p == name[0] && _pattern[j - 1] == '*' || p == '*') && dpPre;
                for (var i = 1; i < name.Length; i++)
                {
                    ifPreAny |= dp[i];
                    bool dpp = dpPre;
                    dpPre = dp[i];
                    dp[i] = name[i] == p && dpp || p == '*' && ifPreAny;
                }
            }

            return dp[dp.Length - 1];
        }
    }
}
