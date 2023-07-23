using Rougamo.Fody.Signature.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Signature.Patterns
{
    public class DefaultTypePattern : IIntermediateTypePattern
    {
        private ITypePattern? _compiledPattern;

        public DefaultTypePattern(TokenSource tokens)
        {
            Tokens = tokens;
        }

        public TokenSource Tokens { get; private set; }

        public bool IsAny => _compiledPattern!.IsAny;

        public bool IsVoid => _compiledPattern!.IsVoid;

        public bool AssignableMatch => _compiledPattern!.AssignableMatch;

        public bool IsMatch(TypeSignature signature)
        {
            return _compiledPattern!.IsMatch(signature);
        }

        public GenericNamePattern SeparateOutMethod()
        {
            var index = Tokens.End - 1;
            var pattern = ExtractGenericNamePattern(Tokens, ref index, "TM", Token.DOT, Token.ELLIPSIS);

            if (Tokens.Start > index)
            {
                Tokens = Tokens.Slice(Tokens.Start, Tokens.Start);
            }
            else
            {
                var token = Tokens.Tokens[index];
                if (token.IsDot())
                {
                    Tokens = Tokens.Slice(Tokens.Start, index);
                }
                else if (token.IsEllipsis())
                {
                    Tokens = Tokens.Slice(Tokens.Start, index + 1);
                }
            }

            return pattern;
        }

        public DeclaringTypeMethodPattern ToDeclaringTypeMethod(params string[] methodImplicitPrefixes)
        {
            var method = SeparateOutMethod();
            method.ImplicitPrefixes = methodImplicitPrefixes;
            return new DeclaringTypeMethodPattern(this, method);
        }

        public void Compile(List<GenericParameterTypePattern> genericParameters, bool genericIn)
        {
            if (Tokens.Start == Tokens.End)
            {
                _compiledPattern = CompiledTypePattern.NewAny();
            }
            else
            {
                var index = Tokens.End - 1;
                _compiledPattern = genericIn ? CompileGenericIn(genericParameters, Tokens, ref index) : CompileGenericOut(genericParameters, Tokens);
            }
        }

        private GenericNamePattern ExtractGenericNamePattern(TokenSource tokens, ref int index, string genericPrefix, params StringOrChar[] stopTokens)
        {
            var inGeneric = false;
            ITypePatterns? genericPatterns = null;
            Stack<IIntermediateTypePattern>? genericParameters = null;
            var nameTokens = new Stack<Token>();
            for (; index >= tokens.Start; index--)
            {
                var token = tokens.Tokens[index];
                if (token.IsGT())
                {
                    if (inGeneric) throw new ArgumentException($"Cannot extract method name pattern, detected nested method generic parameter from pattern({tokens.Value})");
                    if (genericParameters != null) throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters from pattern({tokens.Value})");
                    inGeneric = true;
                    genericParameters = new();
                }
                else if (token.IsLT())
                {
                    if (!inGeneric || genericPatterns != null) throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters format from pattern({tokens.Value})");
                    genericParameters!.Push(new AnyTypePattern());
                    genericPatterns = new TypePatterns(genericParameters.ToArray());
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
                        if (genericParameters!.Count != 0) throw new ArgumentException($"Cannot extract method name pattern, method generic parameter list cannot contains other element if there is a '..' in generic parameter list, pattern({tokens.Value})");
                        token = tokens.Tokens[--index];
                        if (!token.IsLT()) throw new ArgumentException($"Cannot extract method name pattern, method generic parameter list cannot contains other element if there is a '..' in generic parameter list, pattern({tokens.Value})");
                        genericPatterns = new OnePlusTypePatterns();
                        inGeneric = false;
                    }
                    else if (token.IsNot())
                    {
                        if (genericParameters!.Count != 0) throw new ArgumentException($"Cannot extract method name pattern, method generic parameter list cannot contains other element if there is a '!' in generic parameter list, pattern({tokens.Value})");
                        token = tokens.Tokens[--index];
                        if (!token.IsLT()) throw new ArgumentException($"Cannot extract method name pattern, method generic parameter list cannot contains other element if there is a '!' in generic parameter list, pattern({tokens.Value})");
                        genericPatterns = new NoneTypePatterns();
                        inGeneric = false;
                    }
                    else
                    {
                        genericParameters!.Push(new GenericParameterTypePattern(token.Value.ToString()));
                        token = tokens.Tokens[--index];
                        if (token.IsLT())
                        {
                            genericPatterns = new TypePatterns(genericParameters.ToArray());
                            inGeneric = false;
                        }
                        else if (!token.IsComma())
                        {
                            throw new ArgumentException($"Cannot extract method name pattern, unrecognized method generic parameters format from pattern({tokens.Value})");
                        }
                    }
                }
                else if (stopTokens.Any(x => x.Equals(token.Value)))
                {
                    return NewGenericNamePattern(nameTokens, genericPatterns, genericPrefix);
                }
                else
                {
                    nameTokens.Push(token);
                }
            }

            if (nameTokens.Count == 0) throw new ArgumentException($"Cannot extract method name pattern, cannot resolve name part from pattern({Tokens.Value})");

            return NewGenericNamePattern(nameTokens, genericPatterns, genericPrefix);

            static GenericNamePattern NewGenericNamePattern(IReadOnlyCollection<Token> nameTokens, ITypePatterns? genericPatterns, string genericPrefix)
            {
                var name = nameTokens.Count == 1 ? nameTokens.First().Value.ToString() : string.Concat(nameTokens.Select(x => x.Value.ToString()));
                genericPatterns ??= new AnyTypePatterns();
                GenericPatternFactory.FormatVirtualName(genericPatterns, genericPrefix);
                return new GenericNamePattern(name, genericPatterns);
            }
        }

        private ITypePattern CompileGenericOut(List<GenericParameterTypePattern> genericParameters, TokenSource tokens)
        {
            if (tokens.Peek().IsStar() && (tokens.Count == 1 || tokens.Peek(1).IsEllipsis() && (tokens.Count == 2 || tokens.Count == 3 && tokens.Peek(2).IsStar()))) return CompiledTypePattern.NewAny();

            var nestedTypePatterns = new Stack<GenericNamePattern>();
            var nestedDeep = 1;
            var index = tokens.End - 1;
            var assignableMatch = false;
            var nullable = false;
            var token = tokens.Tokens[index];
            if (token.IsPlus())
            {
                assignableMatch = true;
                index--;
            }
            else if (token.IsDoubt())
            {
                nullable = true;
                index--;
            }
            do
            {
                var pattern = ExtractGenericNamePattern(tokens, ref index, $"T{nestedDeep}", TypeSignature.NESTED_SEPARATOR, Token.DOT, Token.ELLIPSIS);
                nestedTypePatterns.Push(pattern);
                nestedDeep++;
            } while (index >= tokens.Start && tokens.Tokens[index--].Value == TypeSignature.NESTED_SEPARATOR);

            INamespacePattern ns = new AnyNamespacePattern();
            if (index >= tokens.Start)
            {
                index = tokens.Tokens[index + 1].IsEllipsis() ? index + 2 : index + 1;
                if (tokens.Start + 2 != index || !tokens.Tokens[tokens.Start].IsStar() && !tokens.Tokens[tokens.Start + 1].IsEllipsis())
                {
                    ns = new NamespacePattern(tokens.Slice(tokens.Start, index));
                }
            }
            var patterns = new GenericNamePatterns(nestedTypePatterns.ToArray());
            patterns.ExtractGenerics(genericParameters);
            var compiledTypePattern = new CompiledTypePattern(ns, patterns, assignableMatch);
            return nullable ? new NullableTypePattern(compiledTypePattern) : compiledTypePattern;
        }

        private ITypePattern CompileGenericIn(List<GenericParameterTypePattern> genericParameters, TokenSource tokens, ref int index)
        {
            if (tokens.Count == 1 && tokens.Peek().IsStar()) return CompiledTypePattern.NewAny();

            var inGeneric = false;
            var assignableMatch = false;
            var nullable = false;
            var token = tokens.Tokens[index];
            if (token.IsPlus())
            {
                assignableMatch = true;
                index--;
            }
            else if (token.IsDoubt())
            {
                nullable = true;
                index--;
            }
            var nameTokens = new Stack<Token>();
            var patterns = new Stack<GenericNamePattern>();
            Stack<ITypePattern>? generics = null;
            ITypePatterns? genericPatterns = null;
            for (; index >= tokens.Start; index--)
            {
                token = tokens.Tokens[index];
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
                    genericPatterns = new TypePatterns(generics.ToArray());
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
                        genericPatterns = new OnePlusTypePatterns();
                        inGeneric = false;
                    }
                    else if (token.IsNot())
                    {
                        if (generics!.Count != 0) throw new ArgumentException($"Cannot extract method name pattern, method generic parameter list cannot contains other element if there is an ellipsis in generic parameter list, pattern({tokens.Value})");
                        token = tokens.Tokens[--index];
                        if (!token.IsLT()) throw new ArgumentException($"Cannot extract method name pattern, method generic parameter list cannot contains other element if there is an ellipsis in generic parameter list, pattern({tokens.Value})");
                        genericPatterns = new NoneTypePatterns();
                        inGeneric = false;
                    }
                    else if (token.IsPlus())
                    {
                        generics!.Push(CompileGenericIn(genericParameters, tokens, ref index));
                    }
                    else
                    {
                        var preToken = tokens.Tokens[index - 1];
                        var generic = genericParameters.SingleOrDefault(x => x.Name == token.Value);
                        if (preToken.IsLT() || preToken.IsComma())
                        {
                            if (generic != null)
                            {
                                generics!.Push(generic);
                            }
                            else
                            {
                                generics!.Push(CompiledTypePattern.NewPrimitiveOrAnyNs(token.Value.ToString(), assignableMatch));
                            }
                            index--;
                            if (preToken.IsLT())
                            {
                                genericPatterns = new TypePatterns(generics.ToArray());
                                inGeneric = false;
                            }
                        }
                        else
                        {
                            generics!.Push(CompileGenericIn(genericParameters, tokens, ref index));
                        }
                    }
                }
                else if (token.Value == TypeSignature.NESTED_SEPARATOR)
                {
                    patterns.Push(CreateGenericNamePattern(nameTokens.ToArray(), genericPatterns));
                }
                else if (token.IsDot())
                {
                    tokens = tokens.Slice(tokens.Start, index);
                    patterns.Push(CreateGenericNamePattern(nameTokens.ToArray(), genericPatterns));
                    break;
                }
                else if (token.IsEllipsis())
                {
                    tokens = tokens.Slice(tokens.Start, index + 1);
                    patterns.Push(CreateGenericNamePattern(nameTokens.ToArray(), genericPatterns));
                    break;
                }
                else
                {
                    nameTokens.Push(token);
                }
            }

            ITypePattern compiledTypePattern;
            if (index + 1 == tokens.Start)
            {
                var pattern = CreateGenericNamePattern(nameTokens.ToArray(), genericPatterns);
                patterns.Push(pattern);
                if (patterns.Count == 1 && pattern.GenericPatterns is AnyTypePatterns)
                {
                    var generic = genericParameters.SingleOrDefault(x => x.Name == pattern.Name);
                    compiledTypePattern = generic ?? (ITypePattern)CompiledTypePattern.NewPrimitiveOrAnyNs(pattern.Name, assignableMatch);
                }
                else
                {
                    compiledTypePattern = new CompiledTypePattern(new AnyNamespacePattern(), new GenericNamePatterns(patterns.ToArray()), assignableMatch);
                }
            }
            else
            {
                compiledTypePattern = new CompiledTypePattern(new NamespacePattern(tokens), new GenericNamePatterns(patterns.ToArray()), assignableMatch);
            }

            return nullable ? new NullableTypePattern(compiledTypePattern) : compiledTypePattern;

            static GenericNamePattern CreateGenericNamePattern(Token[] nameTokens, ITypePatterns? genericPatterns)
            {
                var name = nameTokens.Length == 1 ? nameTokens[0].Value.ToString() : string.Concat(nameTokens.Select(x => x.Value.ToString()));
                genericPatterns ??= new AnyTypePatterns();
                return new GenericNamePattern(name, genericPatterns);
            }
        }
    }
}
