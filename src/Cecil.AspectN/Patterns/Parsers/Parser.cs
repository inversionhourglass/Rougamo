using Cecil.AspectN.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cecil.AspectN.Patterns.Parsers
{
    public class Parser
    {
        private static readonly Dictionary<string, ModifierPattern.Flags> _Modifiers = Enum.GetNames(typeof(ModifierPattern.Flags)).ToDictionary(x => x.ToLower(), x => (ModifierPattern.Flags)Enum.Parse(typeof(ModifierPattern.Flags), x));

        public static ModifierPattern ParseModifier(TokenSource tokens)
        {
            var required = ModifierPattern.Flags.None;
            var forbidden = ModifierPattern.Flags.None;
            Token? token;
            while ((token = tokens.Peek()) != null)
            {
                var offset = 0u;
                if (token.IsNot())
                {
                    token = tokens.Peek(1);
                    if (token == null) break;
                    offset = 1;
                }
                if (!_Modifiers.TryGetValue(token.ToString().ToLower(), out var value)) break;

                if (offset == 0)
                {
                    required |= value;
                }
                else
                {
                    forbidden |= value;
                }
                tokens.Read(offset);
            }

            return new ModifierPattern(required, forbidden);
        }

        public static bool ParseAsync(TokenSource tokens)
        {
            var token = tokens.Peek();
            if (token != null && token.Value == "async")
            {
                tokens.Read();
                return true;
            }
            return false;
        }

        public static ITypePatterns ParseParameters(TokenSource tokens)
        {
            var token = tokens.Read();
            if (!token.IsLBracket()) throw new ArgumentException($"Unknow method parameter definition of pattern({tokens.Value})");
            token = tokens.Peek();
            if (token.IsEllipsis())
            {
                if (!tokens.Read(1).IsRBracket()) throw new ArgumentException($"Unknow method parameter definition of pattern({tokens.Value})");
                return new AnyTypePatterns();
            }
            var typePatterns = new List<IIntermediateTypePattern>();
            do
            {
                token = tokens.Peek();
                if (token.IsComma() || token.IsRBracket())
                {
                    typePatterns.Add(new AnyTypePattern());
                }
                else
                {
                    typePatterns.Add(ParseType(tokens));
                }
            } while (!tokens.Read().IsRBracket());
            return new IntermediateTypePatterns(typePatterns.ToArray());
        }

        public static IIntermediateTypePattern ParseType(TokenSource tokens)
        {
            var typePattern = ParseSingleType(tokens);
            var token = tokens.Peek();
            if (token.IsAnd())
            {
                tokens.Read();
                typePattern = new AndTypePattern(typePattern, ParseNotOrType(tokens));
            }
            if (token.IsOr())
            {
                tokens.Read();
                typePattern = new OrTypePattern(typePattern, ParseType(tokens));
            }

            return typePattern;
        }

        public static IIntermediateTypePattern ParseNotOrType(TokenSource tokens)
        {
            var typePattern = ParseSingleType(tokens);
            var token = tokens.Peek();
            if (token.IsAnd())
            {
                tokens.Read();
                typePattern = new AndTypePattern(typePattern, ParseNotOrType(tokens));
            }

            return typePattern;
        }

        public static IIntermediateTypePattern ParseSingleType(TokenSource tokens)
        {
            var start = tokens.Index;
            var token = tokens.Read();
            if (token == null) throw new ArgumentException($"Unrecognized pattern({tokens}), try parse type but read end of pattern");

            if (token.IsNot()) return new NotTypePattern(ParseType(tokens));

            if (token.IsLBracket()) return ParseTupleTypePattern(tokens);

            if (token.IsNull()) return new NullTypePattern();

            var insideGeneric = 0;
            var inArray = false;
            var pre = token;
            do
            {
                token = tokens.Peek();
                if (token == null || insideGeneric == 0 && !inArray && (token.IsLBracket() || token.IsRBracket() || token.IsComma() || token.IsAnd() || token.IsOr() || pre.End != token.Start)) return new DefaultTypePattern(tokens.Slice(start, tokens.Index));
                if (token.IsLT()) insideGeneric++;
                else if (token.IsGT()) insideGeneric--;
                else if (token.IsLSBracket()) inArray = true;
                else if (token.IsRSBracket()) inArray = false;
                tokens.Read();
                pre = token;
            } while (true);
        }

        private static TupleTypePattern ParseTupleTypePattern(TokenSource tokens)
        {
            var tupleItems = new List<IIntermediateTypePattern>();
            Token? token;
            while ((token = tokens.Peek()) != null)
            {
                if (token.IsLBracket())
                {
                    tokens.Read();
                    tupleItems.Add(ParseTupleTypePattern(tokens));
                    continue;
                }
                if (token.IsRBracket())
                {
                    tokens.Read();
                    return new TupleTypePattern(tupleItems.ToArray());
                }
                if (token.IsComma())
                {
                    tokens.Read();
                    continue;
                }
                tupleItems.Add(ParseType(tokens));
            }
            throw new ArgumentException($"Unexpectedly ended while parsing a tuple type, pattern value is '{tokens.Value}'");
        }
    }
}
