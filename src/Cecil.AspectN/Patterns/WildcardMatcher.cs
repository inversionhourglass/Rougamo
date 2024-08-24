using Cecil.AspectN.Tokens;
using System.Linq;

namespace Cecil.AspectN.Patterns
{
    public static class WildcardMatcher
    {
        public static bool IsMatch(string pattern, string value)
        {
            if (pattern == "*") return true;

            var nonStarCount = pattern.Count(x => x != Token.STAR);

            if (nonStarCount > value.Length) return false;
            if (nonStarCount == pattern.Length) return pattern == value;

            int i = 0, j = 0;
            int iStar = -1, jStar = -1;

            while (i < value.Length)
            {
                if (j < pattern.Length && (pattern[j] == value[i]))
                {
                    i++;
                    j++;
                }
                else if (j < pattern.Length && pattern[j] == '*')
                {
                    iStar = i;
                    jStar = j + 1;
                    j++;
                }
                else if (iStar >= 0)
                {
                    i = ++iStar;
                    j = jStar;
                }
                else
                {
                    return false;
                }
            }

            while (j < pattern.Length && pattern[j] == '*')
            {
                j++;
            }

            return j == pattern.Length;
        }
    }
}
