namespace Rougamo.Fody.Signature.Tokens
{
    public class Token
    {
        public Token(StringOrChar value)
        {
            Value = value;
        }

        public StringOrChar Value { get; }
    }

    public static class TokenExtensions
    {
        private const string REGEX = "regex";
        private const string EXECUTION = "execution";
        private const string METHOD = "method";
        private const string GETTER = "getter";
        private const string SETTER = "setter";
        private const string PROPERTY = "property";

        public static bool IsRegex(this Token token) => token.Value == REGEX;

        public static bool IsExecution(this Token token) => token.Value == EXECUTION;

        public static bool IsMethod(this Token token) => token.Value == METHOD;

        public static bool IsGetter(this Token token) => token.Value == GETTER;

        public static bool IsSetter(this Token token) => token.Value == SETTER;

        public static bool IsProperty(this Token token) => token.Value == PROPERTY;
    }
}
