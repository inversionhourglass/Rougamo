namespace Rougamo.Fody.Signature.Tokens
{
    public class TokenSource
    {
        private int _index;

        public TokenSource(Token[] tokens)
        {
            Tokens = tokens;
        }

        public Token[] Tokens { get; }

        public Token? Peek()
        {
            if (_index >= Tokens.Length) return null;
            return Tokens[_index];
        }

        public Token? Read()
        {
            if (_index >= Tokens.Length) return null;
            return Tokens[_index++];
        }
    }
}
