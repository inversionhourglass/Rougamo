using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Signature.Patterns
{
    public class ArrayTypePattern : ITypePattern
    {
        public ArrayTypePattern(ITypePattern pattern, IReadOnlyCollection<int> ranks)
        {
            ElementPattern = pattern;
            Ranks = ranks.ToArray();
        }

        public ITypePattern ElementPattern { get; }

        public bool IsAny => false;

        public bool IsVoid => false;

        public bool AssignableMatch => ElementPattern.AssignableMatch;

        public int[] Ranks { get; }

        public bool IsMatch(TypeSignature signature)
        {
            if (Ranks.Length != signature.ArrayRanks.Length) return false;

            for (int i = 0; i < Ranks.Length; i++)
            {
                if (Ranks[i] != signature.ArrayRanks[i]) return false;
            }

            return ElementPattern.IsMatch(signature);
        }
    }
}
