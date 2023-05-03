namespace Rougamo.Fody.Signature.Tokens
{
    public class StringOrChar
    {
        private readonly string? _str;
        private readonly char _ch;

        public StringOrChar(string str)
        {
            _str = str;
        }

        public StringOrChar(char ch)
        {
            _ch = ch;
        }

        public static implicit operator StringOrChar(string value)
        {
            return new StringOrChar(value);
        }

        public static implicit operator StringOrChar(char value)
        {
            return new StringOrChar(value);
        }

        public static implicit operator string?(StringOrChar value)
        {
            return value._str;
        }

        public static implicit operator char(StringOrChar value)
        {
            return value._ch;
        }
    }
}
