namespace Cecil.AspectN.Patterns
{
    public class TypePatterns : ITypePatterns
    {
        public TypePatterns(ITypePattern[] patterns)
        {
            Patterns = patterns;
        }

        public ITypePattern[] Patterns { get; }

        public CollectionCount Count => Patterns.Length;

        public bool IsMatch(TypeSignature[] signature)
        {
            if (signature.Length != Patterns.Length) return false;

            for (var i = 0; i < Patterns.Length; i++)
            {
                if (!Patterns[i].IsMatch(signature[i])) return false;
            }

            return true;
        }
    }
}
