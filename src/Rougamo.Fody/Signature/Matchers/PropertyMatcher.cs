namespace Rougamo.Fody.Signature.Matchers
{
    public class PropertyMatcher : OrMatcher
    {
        public PropertyMatcher(string pattern) : base(new GetterMatcher(pattern), new SetterMatcher(pattern))
        {
            Pattern = pattern;
        }

        public string Pattern { get; }

        public override string ToString()
        {
            return $"property({Pattern})";
        }
    }
}
