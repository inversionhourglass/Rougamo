namespace Rougamo.Fody.Signature.Patterns
{
    public class NullableTypePattern : ITypePattern
    {
        public NullableTypePattern(ITypePattern pattern)
        {
            ValuePattern = pattern;
        }

        public ITypePattern ValuePattern { get; }

        public bool IsAny => false;

        public bool AssignableMatch => ValuePattern.AssignableMatch;

        public bool IsMatch(TypeSignature signature)
        {
            if (signature.NestedTypes.Length != 1) return false;

            var valueSignature = signature.NestedTypes[0];
            if (valueSignature.Generics.Length != 1) return false;

            if (signature.Namespace != "System" || valueSignature.Name != "Nullable") return false;

            return ValuePattern.IsMatch(valueSignature.Generics[0]);
        }
    }
}
