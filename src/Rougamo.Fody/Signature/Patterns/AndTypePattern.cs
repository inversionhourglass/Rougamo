namespace Rougamo.Fody.Signature.Patterns
{
    public class AndTypePattern : TypePattern
    {
        public AndTypePattern(TypePattern left, TypePattern right)
        {
            Left = left;
            Right = right;
        }

        public TypePattern Left { get; }

        public TypePattern Right { get; }

        public override GenericNamePattern ExtractNamePattern()
        {
            return Right.ExtractNamePattern();
        }

        public override bool IsMatch(TypeSignature signature)
        {
            return Left.IsMatch(signature) && Right.IsMatch(signature);
        }
    }
}
