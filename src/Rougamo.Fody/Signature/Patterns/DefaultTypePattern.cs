using Rougamo.Fody.Signature.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Signature.Patterns
{
    public class DefaultTypePattern : IIntermediateTypePattern
    {
        private CompiledTypePattern? _compiledPattern;

        public DefaultTypePattern(TokenSource tokens)
        {
            Tokens = tokens;
        }

        public TokenSource Tokens { get; private set; }

        public bool AssignableMatch => _compiledPattern!.AssignableMatch;

        public bool IsMatch(TypeSignature signature)
        {
            return _compiledPattern!.IsMatch(signature);
        }

        public GenericNamePattern ExtractNamePattern()
        {
            var index = Tokens.End - 1;
            var pattern = ExtractGenericNamePattern(Tokens, ref index, "TM", Token.DOT, Token.ELLIPSIS);

            var token = Tokens.Tokens[index];
            if (token.IsDot())
            {
                Tokens = Tokens.Slice(Tokens.Start, index);
            }
            else if (token.IsEllipsis())
            {
                Tokens = Tokens.Slice(Tokens.Start, index + 1);
            }

            return pattern;

            //var inGeneric = false;
            //Stack<TypePattern>? genericParameters = null;
            //var nameTokens = new List<Token>();
            //for (var i = Tokens.End - 1; i >= Tokens.Start; i--)
            //{
            //    var token = Tokens.Tokens[i];
            //    if (token.IsGT())
            //    {
            //        if (inGeneric) throw new ArgumentException($"Cannot extract method name pattern, detected nested method generic parameter from pattern({Tokens.Value})");
            //        if (genericParameters != null) throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters from pattern({Tokens.Value})");
            //        inGeneric = true;
            //        genericParameters = new();
            //    }
            //    else if (token.IsLT())
            //    {
            //        if (!inGeneric) throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters format from pattern({Tokens.Value})");
            //        if (genericParameters!.Count == 0)
            //        {
            //            genericParameters.Push(new AnyTypePattern());
            //        }
            //        inGeneric = false;
            //    }
            //    else if (inGeneric)
            //    {
            //        if (token.IsComma())
            //        {
            //            genericParameters!.Push(new AnyTypePattern());
            //        }
            //        else if (token.IsEllipsis())
            //        {
            //            if (genericParameters!.Count != 0) throw new ArgumentException($"Cannot extract method name pattern, method generic parameter list cannot contains other element if there is an ellipsis in generic parameter list, pattern({Tokens.Value})");
            //            token = Tokens.Tokens[--i];
            //            if (!token.IsLT()) throw new ArgumentException($"Cannot extract method name pattern, method generic parameter list cannot contains other element if there is an ellipsis in generic parameter list, pattern({Tokens.Value})");
            //        }
            //        else
            //        {
            //            genericParameters!.Push(new GenericParameterTypePattern(token.Value.ToString()));
            //            token = Tokens.Tokens[i - 1];
            //            if (token.IsComma()) i--;
            //            else if (token.IsLT()) continue;
            //            else throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters format from pattern({Tokens.Value})");
            //        }
            //    }
            //    else if (token.IsDot())
            //    {
            //        Tokens = Tokens.Slice(Tokens.Start, i);
            //        var name = nameTokens.Count == 1 ? nameTokens[0].Value.ToString() : string.Concat(nameTokens.Select(x => x.Value.ToString()));
            //        return new GenericNamePattern(name, GenericPatternFactory.New(genericParameters, "TM"));
            //    }
            //    else if (token.IsEllipsis())
            //    {
            //        Tokens = Tokens.Slice(Tokens.Start, i + 1);
            //        var name = nameTokens.Count == 1 ? nameTokens[0].Value.ToString() : string.Concat(nameTokens.Select(x => x.Value.ToString()));
            //        return new GenericNamePattern(name, GenericPatternFactory.New(genericParameters, "TM"));
            //    }
            //    else
            //    {
            //        nameTokens.Add(token);
            //    }
            //}

            //throw new ArgumentException($"Cannot extract method name pattern, cannot resolve name part from pattern({Tokens.Value})");
        }

        public void Compile(List<GenericParameterTypePattern> genericParameters, bool genericIn)
        {
            var index = Tokens.End - 1;
            _compiledPattern = genericIn ? CompileGenericIn(genericParameters, Tokens, ref index) : CompileGenericOut(genericParameters, Tokens);
        }

        private GenericNamePattern ExtractGenericNamePattern(TokenSource tokens, ref int index, string genericPrefix, params StringOrChar[] stopTokens)
        {
            var inGeneric = false;
            Stack<IIntermediateTypePattern>? genericParameters = null;
            var nameTokens = new List<Token>();
            for (var i = index; i >= tokens.Start; i--)
            {
                var token = tokens.Tokens[i];
                if (token.IsGT())
                {
                    if (inGeneric) throw new ArgumentException($"Cannot extract method name pattern, detected nested method generic parameter from pattern({tokens.Value})");
                    if (genericParameters != null) throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters from pattern({tokens.Value})");
                    inGeneric = true;
                    genericParameters = new();
                }
                else if (token.IsLT())
                {
                    if (!inGeneric) throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters format from pattern({tokens.Value})");
                    if (genericParameters!.Count == 0)
                    {
                        genericParameters.Push(new AnyTypePattern());
                    }
                    inGeneric = false;
                }
                else if (inGeneric)
                {
                    if (token.IsComma())
                    {
                        genericParameters!.Push(new AnyTypePattern());
                    }
                    else if (token.IsEllipsis())
                    {
                        if (genericParameters!.Count != 0) throw new ArgumentException($"Cannot extract method name pattern, method generic parameter list cannot contains other element if there is an ellipsis in generic parameter list, pattern({tokens.Value})");
                        token = tokens.Tokens[--i];
                        if (!token.IsLT()) throw new ArgumentException($"Cannot extract method name pattern, method generic parameter list cannot contains other element if there is an ellipsis in generic parameter list, pattern({tokens.Value})");
                    }
                    else
                    {
                        genericParameters!.Push(new GenericParameterTypePattern(token.Value.ToString()));
                        token = tokens.Tokens[i - 1];
                        if (token.IsComma()) i--;
                        else if (token.IsLT()) continue;
                        else throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters format from pattern({tokens.Value})");
                    }
                }
                else if (stopTokens.Any(x => x.Equals(token.Value)))
                {
                    var name = nameTokens.Count == 1 ? nameTokens[0].Value.ToString() : string.Concat(nameTokens.Select(x => x.Value.ToString()));
                    return new GenericNamePattern(name, GenericPatternFactory.New(genericParameters, genericPrefix));
                }
                else
                {
                    nameTokens.Add(token);
                }
            }

            throw new ArgumentException($"Cannot extract method name pattern, cannot resolve name part from pattern({Tokens.Value})");
        }

        private CompiledTypePattern CompileGenericOut(List<GenericParameterTypePattern> genericParameters, TokenSource tokens)
        {
            if (tokens.Count == 1 && tokens.Peek().IsStar() || tokens.Count == 3 && tokens.Peek().IsStar() && tokens.Peek(1).IsEllipsis() && tokens.Peek(2).IsStar()) return CompiledTypePattern.NewAny();

            var nestedTypePatterns = new Stack<GenericNamePattern>();
            var nestedDeep = 1;
            var index = tokens.End - 1;
            var assignableMatch = false;
            if (tokens.Tokens[index].IsPlus())
            {
                assignableMatch = true;
                index--;
            }
            do
            {
                var pattern = ExtractGenericNamePattern(tokens, ref index, $"T{nestedDeep}", TypeSignature.NESTED_SEPARATOR, Token.DOT, Token.ELLIPSIS);
                nestedTypePatterns.Push(pattern);
                nestedDeep++;
            } while (tokens.Tokens[index--].Value == TypeSignature.NESTED_SEPARATOR);

            var ns = new NamespacePattern(tokens.Slice(tokens.Start, tokens.Index));
            var patterns = new GenericNamePatterns(nestedTypePatterns.ToArray());
            patterns.ExtractGenerics(genericParameters);
            return new CompiledTypePattern(ns, patterns, assignableMatch);
        }

        private CompiledTypePattern CompileGenericIn(List<GenericParameterTypePattern> genericParameters, TokenSource tokens, ref int index)
        {
            var inGeneric = false;
            var assignableMatch = false;
            if (tokens.Tokens[index].IsPlus())
            {
                assignableMatch = true;
                index--;
            }
            var nameTokens = new List<Token>();
            var patterns = new Stack<GenericNamePattern>();
            Stack<ITypePattern>? generics = null;
            for (; index >= tokens.Start; index--)
            {
                var token = tokens.Tokens[index];
                if (token.IsGT())
                {
                    if (inGeneric)
                    {
                        generics!.Push(CompileGenericIn(genericParameters, tokens, ref index));
                    }
                    else
                    {
                        if (generics != null) throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters from pattern({tokens.Value})");
                        inGeneric = true;
                        generics = new();
                    }
                }
                else if (token.IsLT())
                {
                    if (!inGeneric) throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters format from pattern({tokens.Value})");
                    if (generics!.Count == 0)
                    {
                        generics!.Push(new AnyTypePattern());
                    }
                    inGeneric = false;
                }
                else if (inGeneric)
                {
                    if (token.IsComma())
                    {
                        generics!.Push(new AnyTypePattern());
                    }
                    else if (token.IsEllipsis())
                    {
                        if (generics!.Count != 0) throw new ArgumentException($"Cannot extract method name pattern, method generic parameter list cannot contains other element if there is an ellipsis in generic parameter list, pattern({tokens.Value})");
                        token = tokens.Tokens[--index];
                        if (!token.IsLT()) throw new ArgumentException($"Cannot extract method name pattern, method generic parameter list cannot contains other element if there is an ellipsis in generic parameter list, pattern({tokens.Value})");
                    }
                    else if (token.IsPlus())
                    {
                        generics!.Push(CompileGenericIn(genericParameters, tokens, ref index));
                    }
                    else
                    {
                        var preToken = tokens.Tokens[index - 1];
                        var generic = genericParameters.SingleOrDefault(x => x.Name == token.Value);
                        if (preToken.IsComma() && generic != null)
                        {
                            generics!.Push(generic);
                            index--;
                        }
                        else if (preToken.IsLT() || preToken.IsComma())
                        {
                            generics!.Push(CompiledTypePattern.NewPrimitiveOrAnyNs(token.Value.ToString(), assignableMatch));
                        }
                        else
                        {
                            generics!.Push(CompileGenericIn(genericParameters, tokens, ref index));
                        }
                    }
                }
                else if (token.Value == TypeSignature.NESTED_SEPARATOR)
                {
                    var name = nameTokens.Count == 1 ? nameTokens[0].Value.ToString() : string.Concat(nameTokens.Select(x => x.Value.ToString()));
                    patterns.Push(new GenericNamePattern(name, GenericPatternFactory.New(generics, null)));
                }
                else if (token.IsDot())
                {
                    tokens = tokens.Slice(tokens.Start, index);
                    break;
                }
                else if (token.IsEllipsis())
                {
                    tokens = tokens.Slice(tokens.Start, index + 1);
                    break;
                }
                else
                {
                    nameTokens.Add(token);
                }
            }

            INamespacePattern namespacePattern = tokens.Count == 0 ? new AnyNamespacePattern() : new NamespacePattern(tokens);
            return new CompiledTypePattern(namespacePattern, new GenericNamePatterns(patterns.ToArray()), assignableMatch);
        }
    }
}
