namespace Rougamo.Fody.Signature.Tokens
{
    public class Token
    {
        public const string REGEX = "regex";
        public const string EXECUTION = "execution";
        public const string METHOD = "method";
        public const string GETTER = "getter";
        public const string SETTER = "setter";
        public const string PROPERTY = "property";
        public const char SPACE = ' ';
        public const char NOT = '!';

        public static readonly Token Space = new(SPACE);

        public Token(StringOrChar value)
        {
            Value = value;
        }

        public StringOrChar Value { get; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public static class TokenExtensions
    {
        public static bool IsRegex(this Token token) => token.Value == Token.REGEX;

        public static bool IsExecution(this Token token) => token.Value == Token.EXECUTION;

        public static bool IsMethod(this Token token) => token.Value == Token.METHOD;

        public static bool IsGetter(this Token token) => token.Value == Token.GETTER;

        public static bool IsSetter(this Token token) => token.Value == Token.SETTER;

        public static bool IsProperty(this Token token) => token.Value == Token.PROPERTY;

        public static bool IsSpace(this Token token) => token.Value == Token.SPACE;

        public static bool IsNot(this Token token) => token.Value == Token.NOT;
    }
}
