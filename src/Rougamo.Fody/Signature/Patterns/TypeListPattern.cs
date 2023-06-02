namespace Rougamo.Fody.Signature.Patterns
{
    public abstract class TypeListPattern
    {
        public abstract bool IsMatch(TypeSignature[] signature);
    }
}
