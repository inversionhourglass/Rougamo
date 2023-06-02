namespace Rougamo.Fody.Signature.Patterns
{
    public class AnyTypeListPattern : TypeListPattern
    {
        public override bool IsMatch(TypeSignature[] signature) => true;
    }
}
