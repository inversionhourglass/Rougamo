namespace Rougamo.Fody.Signature.Patterns
{
    public class RawNamePattern : NamePattern
    {
        private readonly string _name;

        public RawNamePattern(string name)
        {
            _name = name;
        }

        public override bool IsMatch(string name) => _name == name;
    }
}
