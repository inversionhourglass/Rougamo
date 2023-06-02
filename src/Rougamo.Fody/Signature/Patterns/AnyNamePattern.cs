namespace Rougamo.Fody.Signature.Patterns
{
    public class AnyNamePattern : NamePattern
    {
        public override bool IsMatch(string name) => true;
    }
}
