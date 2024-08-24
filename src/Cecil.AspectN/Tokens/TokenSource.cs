namespace Cecil.AspectN.Tokens
{
    public class TokenSource
    {
        private readonly int _start;
        private readonly int _end;
        private int _index;

        public TokenSource(string value, Token[] tokens) : this(value, tokens, 0, tokens.Length) { }

        public TokenSource(string value, Token[] tokens, int start, int end)
        {
            _index = start;
            _start = start;
            _end = end;
            Value = value;
            Tokens = tokens;
        }

        public string Value { get; }

        public int Start => _start;

        public int End => _end;

        public int Index => _index;

        public int Count => _end - _start;

        public Token[] Tokens { get; }

        public Token? Peek(int offset = 0)
        {
            var index = _index + offset;
            if (index >= _end || index < 0) return null;
            return Tokens[index];
        }

        public Token? Read(uint offset = 0)
        {
            _index += (int)offset;
            if (_index >= _end) return null;
            return Tokens[_index++];
        }

        public TokenSource Slice(int start, int end)
        {
            return new TokenSource(Value, Tokens, start, end);
        }

        public override string ToString()
        {
            if (_start == _end) return string.Empty;

            var start = Tokens[_start].Start;
            var end = Tokens[_end - 1].End;
            return Value.Substring(start, end - start);
        }
    }
}
