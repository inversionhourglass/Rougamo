namespace Rougamo.Fody.Signature.Patterns
{
    public class NotTypePattern : TypePattern
    {
        public NotTypePattern(TypePattern innerPattern)
        {
            InnerPattern = innerPattern;
        }

        public TypePattern InnerPattern { get; }

        public override GenericNamePattern ExtractNamePattern()
        {
            return InnerPattern.ExtractNamePattern();
        }

        public override bool IsMatch(TypeSignature signature)
        {
            return !InnerPattern.IsMatch(signature);
        }
    }
}
