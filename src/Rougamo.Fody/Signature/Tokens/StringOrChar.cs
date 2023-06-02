using System;

namespace Rougamo.Fody.Signature.Tokens
{
    public class StringOrChar : IEquatable<StringOrChar>
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

        public override string ToString()
        {
            return _str ?? _ch.ToString();
        }

        public bool Equals(StringOrChar other)
        {
            return _str == other._str && _ch == other._ch;
        }
    }
}
