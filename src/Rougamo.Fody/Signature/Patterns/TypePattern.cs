namespace Rougamo.Fody.Signature.Patterns
{
    public abstract class TypePattern
    {
        public abstract GenericNamePattern ExtractNamePattern();

        public abstract bool IsMatch(TypeSignature signature);
    }
}
