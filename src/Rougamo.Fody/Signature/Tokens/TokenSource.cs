namespace Rougamo.Fody.Signature.Tokens
{
    public class TokenSource
    {
        public TokenSource(Token[] tokens)
        {
            Tokens = tokens;
        }

        public Token[] Tokens { get; }
    }
}
