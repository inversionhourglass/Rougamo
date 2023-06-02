using Rougamo.Fody.Signature.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Signature.Patterns
{
    public class DefaultTypePattern : TypePattern
    {
        private StructuredTypePattern _pattern;

        public DefaultTypePattern(TokenSource tokens)
        {
            Tokens = tokens;
        }

        public TokenSource Tokens { get; private set; }

        public override GenericNamePattern ExtractNamePattern()
        {
            var inGeneric = false;
            List<TypePattern>? genericParameters = null;
            var nameTokens = new List<Token>();
            for (var i = Tokens.End - 1; i >= Tokens.Start; i--)
            {
                var token = Tokens.Tokens[i];
                if (token.IsGT())
                {
                    if (inGeneric) throw new ArgumentException($"Cannot extract method name pattern, detected nested method generic parameter from pattern({Tokens.Value})");
                    if (genericParameters != null) throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters from pattern({Tokens.Value})");
                    inGeneric = true;
                    genericParameters = new();
                }
                else if (token.IsLT())
                {
                    if (!inGeneric) throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters format from pattern({Tokens.Value})");
                    inGeneric = false;
                }
                else if (inGeneric)
                {
                    if (token.IsComma())
                    {
                        genericParameters!.Add(new AnyTypePattern());
                    }
                    else
                    {
                        genericParameters!.Add(new GenericParameterTypePattern(token.Value.ToString()));
                        token = Tokens.Tokens[i - 1];
                        if (token.IsComma()) i--;
                        else if (token.IsLT()) continue;
                        else throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters format from pattern({Tokens.Value})");
                    }
                }
                else if (token.IsDot())
                {
                    Tokens = Tokens.Slice(Tokens.Start, i);
                    var name = nameTokens.Count == 1 ? nameTokens[0].Value.ToString() : string.Concat(nameTokens.Select(x => x.Value.ToString()));
                    return genericParameters == null ? new GenericNamePattern(name, null) : new GenericNamePattern(name, genericParameters.AsEnumerable().Reverse().ToArray());
                }
                else if (token.IsEllipsis())
                {
                    Tokens = Tokens.Slice(Tokens.Start, i + 1);
                    var name = nameTokens.Count == 1 ? nameTokens[0].Value.ToString() : string.Concat(nameTokens.Select(x => x.Value.ToString()));
                    return genericParameters == null ? new GenericNamePattern(name, null) : new GenericNamePattern(name, genericParameters.AsEnumerable().Reverse().ToArray());
                }
                else
                {
                    nameTokens.Add(token);
                }
            }

            throw new ArgumentException($"Cannot extract method name pattern, cannot resolve name part from pattern({Tokens.Value})");
        }

        //public void Ab()
        //{
        //    var inGeneric = false;
        //    List<TypePattern>? genericParameters = null;
        //    var nestedTypes = new List<GenericNamePattern>();
        //    var nameTokens = new List<Token>();
        //    var i = Tokens.End - 1;
        //    for (; i >= Tokens.Start; i--)
        //    {
        //        var token = Tokens.Tokens[i];
        //        if (token.IsGT())
        //        {
        //            if (inGeneric) throw new ArgumentException($"Cannot extract method name pattern, detected nested method generic parameter from pattern({Tokens.Value})");
        //            if (genericParameters != null) throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters from pattern({Tokens.Value})");
        //            inGeneric = true;
        //            genericParameters = new();
        //        }
        //        else if (token.IsLT())
        //        {
        //            if (!inGeneric) throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters format from pattern({Tokens.Value})");
        //            inGeneric = false;
        //        }
        //        else if (inGeneric)
        //        {
        //            if (token.IsComma())
        //            {
        //                genericParameters!.Add(new AnyTypePattern());
        //            }
        //            else
        //            {
        //                genericParameters!.Add(new GenericParameterTypePattern(token.Value.ToString()));
        //                token = Tokens.Tokens[i - 1];
        //                if (token.IsComma()) i--;
        //                else if (token.IsLT()) continue;
        //                else throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters format from pattern({Tokens.Value})");
        //            }
        //        }
        //        else if (token.IsDot())
        //        {
        //            var ns = Tokens.Slice(Tokens.Start, i);
        //            var name = nameTokens.Count == 1 ? nameTokens[0].Value.ToString() : string.Concat(nameTokens.Select(x => x.Value.ToString()));
        //            return genericParameters == null ? new GenericNamePattern(name, null) : new GenericNamePattern(name, genericParameters.AsEnumerable().Reverse().ToArray());
        //        }
        //        else if (token.IsEllipsis())
        //        {
        //            Tokens = Tokens.Slice(Tokens.Start, i + 1);
        //            var name = nameTokens.Count == 1 ? nameTokens[0].Value.ToString() : string.Concat(nameTokens.Select(x => x.Value.ToString()));
        //            return genericParameters == null ? new GenericNamePattern(name, null) : new GenericNamePattern(name, genericParameters.AsEnumerable().Reverse().ToArray());
        //        }
        //        else
        //        {
        //            nameTokens.Add(token);
        //        }
        //    }
        //}

        public override bool IsMatch(TypeSignature signature)
        {
            throw new NotImplementedException();
        }
    }
}
