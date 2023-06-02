namespace Rougamo.Fody.Signature.Patterns
{
    public class AnyTypePattern : TypePattern
    {
        public override GenericNamePattern ExtractNamePattern()
        {
            throw new System.NotImplementedException();
        }

        public override bool IsMatch(TypeSignature signature) => true;
    }
}
