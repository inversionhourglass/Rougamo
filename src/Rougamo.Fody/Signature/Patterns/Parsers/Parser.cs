using Rougamo.Fody.Signature.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Signature.Patterns.Parsers
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
                typePatterns.Add(ParseType(tokens));
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

            var insideGeneric = 0;
            var pre = token;
            do
            {
                token = tokens.Peek();
                if (token == null || insideGeneric == 0 && (token.IsLBracket() || token.IsRBracket() || token.IsComma() || pre.End != token.Start)) return new DefaultTypePattern(tokens.Slice(start, tokens.Index));
                if (token.IsLT()) insideGeneric++;
                else if (token.IsGT()) insideGeneric--;
                tokens.Read();
                pre = token;
            } while (true);
        }

        private static TupleTypePattern ParseTupleTypePattern(TokenSource tokens)
        {
            var tupleItems = new List<IIntermediateTypePattern>();
            Token? token;
            while ((token = tokens.Read()) != null)
            {
                if (token.IsLBracket())
                {
                    tupleItems.Add(ParseTupleTypePattern(tokens));
                    continue;
                }
                if (token.IsRBracket()) return new TupleTypePattern(tupleItems.ToArray());
                if (token.IsComma()) continue;
                tupleItems.Add(ParseType(tokens));
            }
            throw new ArgumentException($"Unexpectedly ended while parsing a tuple type, pattern value is '{tokens.Value}'");
        }
    }
}
