using System.Collections.Generic;

namespace Cecil.AspectN.Patterns
{
    public class GenericNamePatterns : IGenericNamePatterns
    {
        public GenericNamePatterns(GenericNamePattern[] genericNamePatterns)
        {
            Patterns = genericNamePatterns;
        }

        public GenericNamePattern[] Patterns { get; set; }

        public CollectionCount Count => Patterns.Length;

        public void ExtractGenerics(List<GenericParameterTypePattern> list)
        {
            foreach (var pattern in Patterns)
            {
                pattern.ExtractGenerics(list);
            }
        }

        public bool IsMatch(GenericSignature[] signatures)
        {
            if (signatures.Length != Patterns.Length) return false;

            for (var i = 0; i < signatures.Length; i++)
            {
                if (!Patterns[i].IsMatch(signatures[i])) return false;
            }

            return true;
        }
    }
}
