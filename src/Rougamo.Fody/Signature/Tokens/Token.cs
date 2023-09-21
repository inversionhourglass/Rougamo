using System;

namespace Rougamo.Fody.Signature.Tokens
{
    public class Token : IEquatable<Token>
    {
        public const string REGEX = "regex";
        public const string EXECUTION = "execution";
        public const string METHOD = "method";
        public const string GETTER = "getter";
        public const string SETTER = "setter";
        public const string PROPERTY = "property";
        public const string NULL = "null";
        public const string ELLIPSIS = "..";
        public const string AND = "&&";
        public const string OR = "||";
        public const char NOT = '!';
        public const char DOUBT = '?';
        public const char STAR = '*';
        public const char PLUS = '+';
        public const char DOT = '.';
        public const char COMMA = ',';
        public const char L_BRACKET = '(';
        public const char R_BRACKET = ')';
        public const char LT = '<';
        public const char GT = '>';

        public Token(StringOrChar value, int start, int end)
        {
            Value = value;
            Start = start;
            End = end;
        }

        public StringOrChar Value { get; }

        public int Start { get; }

        public int End { get; }

        public bool NextTo(Token? token)
        {
            if (token == null) return false;
            return End == token.Start || Start == token.End;
        }

        public bool Equals(Token? other)
        {
            if (other == null) return false;

            return Value.Equals(other.Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public static class TokenExtensions
    {
        public static bool IsRegex(this Token? token) => token != null && token.Value == Token.REGEX;

        public static bool IsExecution(this Token? token) => token != null && token.Value == Token.EXECUTION;

        public static bool IsMethod(this Token? token) => token != null && token.Value == Token.METHOD;

        public static bool IsGetter(this Token? token) => token != null && token.Value == Token.GETTER;

        public static bool IsSetter(this Token? token) => token != null && token.Value == Token.SETTER;

        public static bool IsProperty(this Token? token) => token != null && token.Value == Token.PROPERTY;

        public static bool IsEllipsis(this Token? token) => token != null && token.Value == Token.ELLIPSIS;

        public static bool IsAnd(this Token? token) => token != null && token.Value == Token.AND;

        public static bool IsOr(this Token? token) => token != null && token.Value == Token.OR;

        public static bool IsNot(this Token? token) => token != null && token.Value == Token.NOT;

        public static bool IsDoubt(this Token? token) => token != null && token.Value == Token.DOUBT;

        public static bool IsStar(this Token? token) => token != null && token.Value == Token.STAR;

        public static bool IsPlus(this Token? token) => token != null && token.Value == Token.PLUS;

        public static bool IsDot(this Token? token) => token != null && token.Value == Token.DOT;

        public static bool IsComma(this Token? token) => token != null && token.Value == Token.COMMA;

        public static bool IsLBracket(this Token? token) => token != null && token.Value == Token.L_BRACKET;

        public static bool IsRBracket(this Token? token) => token != null && token.Value == Token.R_BRACKET;

        public static bool IsLT(this Token? token) => token != null && token.Value == Token.LT;

        public static bool IsGT(this Token? token) => token != null && token.Value == Token.GT;

        public static bool IsNull(this Token? token) => token != null && token.Value == Token.NULL;
    }
}
