namespace Rougamo.Fody.Signature.Patterns
{
    public class DefaultTypeListPattern : TypeListPattern
    {
        public DefaultTypeListPattern(TypePattern[] patterns)
        {
            Patterns = patterns;
        }

        public TypePattern[] Patterns { get; }

        public override bool IsMatch(TypeSignature[] signature)
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
