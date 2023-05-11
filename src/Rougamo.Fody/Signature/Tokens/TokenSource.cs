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

        public Token? Peek(int offset = 0)
        {
            var index = _index + offset;
            if (index >= Tokens.Length || index < 0) return null;
            return Tokens[index];
        }

        public Token? Read(uint offset = 0)
        {
            _index += (int)offset;
            if (_index >= Tokens.Length) return null;
            return Tokens[_index++];
        }
    }

    public static class TokenSourceExtensions
    {
        public static bool TryPeekNotSpace(this TokenSource tokens, out Token? token)
        {
            while ((token = tokens.Peek()) != null)
            {
                if (!token.IsSpace()) return true;

                tokens.Read();
            }
            return false;
        }
    }
}
