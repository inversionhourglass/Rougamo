namespace Cecil.AspectN.Matchers
{
    public class NotMatcher : IMatcher
    {
        private readonly IMatcher _matcher;

        public NotMatcher(IMatcher matcher)
        {
            _matcher = matcher;
        }

        public IMatcher InnerMatcher => _matcher;

        public bool IsMatch(MethodSignature signature)
        {
            return !_matcher.IsMatch(signature);
        }

        public override string ToString()
        {
            return $"!{_matcher}";
        }
    }
}
