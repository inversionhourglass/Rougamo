using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Signature
{
    public class SignatureParser
    {
        public static MethodSignature ParseMethod(MethodDefinition methodDef)
        {
            var genericParameters = new List<GenericParameterTypeSignature>();
            var modifier = ParseModifier(methodDef);
            var declareType = ParseType(methodDef.DeclaringType, genericParameters, null);
            var typeGenericCount = genericParameters.Count;
            var method = new GenericSignature(methodDef.Name, ParseMethodGenericParameters(methodDef, genericParameters));
            ParseGenericVirtualNames(declareType);
            ParseGenericVirtualNames(method, "TM");
            var genericMap = SortGenericParameter(typeGenericCount, genericParameters);
            var returnType = ParseType(methodDef.ReturnType, null, genericMap);
            var parameters = ParseMethodParameters(methodDef, genericMap);
            return new MethodSignature(modifier, returnType, declareType, method, parameters);
        }

        private static Modifier ParseModifier(MethodDefinition methodDef)
        {
            var modifier = ParseMethodModifier(methodDef.Attributes);
            var isStatic = modifier & Modifier.Static;
            var declaringType = methodDef.DeclaringType;
            while (declaringType != null)
            {
                modifier &= ParseTypeModifier(declaringType.Attributes);
                declaringType = declaringType.DeclaringType;
            }

            return modifier | isStatic;
        }

        private static Modifier ParseTypeModifier(TypeAttributes attribute)
        {
            if ((attribute & TypeAttributes.NestedFamORAssem) == TypeAttributes.NestedFamORAssem)
            {
                return Modifier.ProtectedInternal;
            }
            if ((attribute & TypeAttributes.NestedFamANDAssem) == TypeAttributes.NestedFamANDAssem)
            {
                return Modifier.PrivateProtected;
            }
            if ((attribute & TypeAttributes.NestedAssembly) == TypeAttributes.NestedAssembly)
            {
                return Modifier.Internal;
            }
            if ((attribute & TypeAttributes.NestedFamily) == TypeAttributes.NestedFamily)
            {
                return Modifier.Protected;
            }
            if ((attribute & TypeAttributes.NestedPrivate) == TypeAttributes.NestedPrivate)
            {
                return Modifier.Private;
            }
            if ((attribute & TypeAttributes.Public) == TypeAttributes.Public || (attribute & TypeAttributes.NestedPublic) == TypeAttributes.NestedPublic)
            {
                return Modifier.Public;
            }

            return Modifier.Internal;
        }

        private static Modifier ParseMethodModifier(MethodAttributes attribute)
        {
            var modifier = (attribute & MethodAttributes.Static) == 0 ? 0 : Modifier.Static;

            if ((attribute & MethodAttributes.Public) == MethodAttributes.Public)
            {
                return modifier | Modifier.Public;
            }
            if ((attribute & MethodAttributes.Assembly) == MethodAttributes.Assembly)
            {
                return modifier | Modifier.Internal;
            }
            if ((attribute & MethodAttributes.FamORAssem) == MethodAttributes.FamORAssem)
            {
                return modifier | Modifier.ProtectedInternal;
            }
            if ((attribute & MethodAttributes.Family) != 0)
            {
                return modifier | Modifier.Protected;
            }
            if ((attribute & MethodAttributes.FamANDAssem) != 0)
            {
                return modifier | Modifier.PrivateProtected;
            }
            if ((attribute & MethodAttributes.Private) != 0)
            {
                return modifier | Modifier.Private;
            }

            throw new ArgumentException($"Unable parse {attribute} to Modifier", nameof(attribute));
        }

        public static TypeSignature ParseType(TypeReference typeRef, List<GenericParameterTypeSignature>? genericParameters, Dictionary<string, TypeSignature>? genericMap)
        {
            if (typeRef is GenericParameter)
            {
                if (genericMap != null && genericMap.TryGetValue(typeRef.Name, out var singature)) return singature;

                var signature = new GenericParameterTypeSignature(typeRef.Name, typeRef);
                genericParameters!.Add(signature);
                return signature;
            }

            if (typeRef is GenericInstanceType git)
            {
                var typeReference = typeRef;
                var i = git.GenericArguments.Count - 1;
                var boundary = git.GenericArguments.Count;
                var nestedTypes = new Stack<GenericSignature>();
                string gitNs;
                do
                {
                    gitNs = typeReference.Namespace;
                    var name = ShortGenericName(typeReference.Name, out var genericCount);
                    var argumentSignatures = new Stack<TypeSignature>();
                    boundary -= genericCount;
                    for (; i >= boundary; i--)
                    {
                        var argumentSignature = ParseType(git.GenericArguments[i], genericParameters, genericMap);
                        argumentSignatures.Push(argumentSignature!);
                    }
                    nestedTypes.Push(new GenericSignature(name, argumentSignatures));
                } while ((typeReference = typeReference.DeclaringType) != null);
                return new TypeSignature(gitNs, nestedTypes, typeRef);
            }

            if (typeRef.HasGenericParameters)
            {
                var typeReference = typeRef;
                var i = typeRef.GenericParameters.Count - 1;
                var boundary = typeRef.GenericParameters.Count;
                var nestedTypes = new Stack<GenericSignature>();
                string gptNs;
                do
                {
                    gptNs = typeReference.Namespace;
                    var name = ShortGenericName(typeReference.Name, out var genericCount);
                    var argumentSignatures = new Stack<TypeSignature>();
                    boundary -= genericCount;
                    for (; i >= boundary; i--)
                    {
                        var argumentSignature = ParseType(typeRef.GenericParameters[i], genericParameters, genericMap);
                        argumentSignatures.Push(argumentSignature!);
                    }
                    nestedTypes.Push(new GenericSignature(name, argumentSignatures));
                } while ((typeReference = typeReference.DeclaringType) != null);
                return new TypeSignature(gptNs, nestedTypes, typeRef);
            }

            var reference = typeRef;
            var nesteds = new Stack<GenericSignature>();
            string @namespace;
            do
            {
                @namespace = reference.Namespace;
                nesteds.Push(new GenericSignature(reference.Name, TypeSignature.EmptyArray));
            } while ((reference = reference.DeclaringType) != null);
            return new TypeSignature(@namespace, nesteds, typeRef);
        }

        private static TypeSignature[] ParseMethodGenericParameters(MethodDefinition methodDef, List<GenericParameterTypeSignature> genericParameters)
        {
            return methodDef.HasGenericParameters ? methodDef.GenericParameters.Select(x => ParseType(x, genericParameters, null)!).ToArray() : TypeSignature.EmptyArray;
        }

        private static TypeSignature[] ParseMethodParameters(MethodDefinition methodDef, Dictionary<string, TypeSignature>? genericNameMap)
        {
            return methodDef.HasParameters ? methodDef.Parameters.Select(x => ParseType(x.ParameterType, null, genericNameMap)!).ToArray() : TypeSignature.EmptyArray;
        }

        private static Dictionary<string, TypeSignature>? SortGenericParameter(int typeGenericCount, List<GenericParameterTypeSignature> genericParameters)
        {
            if (genericParameters.Count == 0) return null;
            if (genericParameters.Count == 1)
            {
                var genericParameter = genericParameters[0];
                genericParameter.SortName = "T";
                return new Dictionary<string, TypeSignature> { { genericParameter.Name, genericParameter } };
            }
            for (var i = 0; i < genericParameters.Count; i++)
            {
                if (i < typeGenericCount)
                {
                    genericParameters[i].SortName = $"T{typeGenericCount - i}";
                }
                else
                {
                    genericParameters[i].SortName = $"T{i + 1}";
                }
            }
            return genericParameters.ToDictionary(x => x.Name, x => (TypeSignature)x);
        }

        private static void ParseGenericVirtualNames(TypeSignature signature)
        {
            if (signature.NestedTypes.Length == 0) return;

            var j = 1;
            for (var i = signature.NestedTypes.Length - 1; i >= 0; i--)
            {
                ParseGenericVirtualNames(signature.NestedTypes[i], $"T{j}");
                j++;
            }
            //for (int i = 0; i < signature.NestedTypes.Length; i++)
            //{
            //    ParseGenericVirtualNames(signature.NestedTypes[i], $"T{i + 1}");
            //}
        }

        private static void ParseGenericVirtualNames(GenericSignature signature, string prefix)
        {
            if (signature.Generics.Length == 0) return;

            for (var i = 0; i < signature.Generics.Length; i++)
            {
                if (signature.Generics[i] is GenericParameterTypeSignature genericParameter)
                {
                    genericParameter.VirtualName = prefix + (i + 1);
                }
            }
        }

        private static string ShortGenericName(string name, out int genericCount)
        {
            var index = name.IndexOf('`');
            if (index == -1) throw new ArgumentException($"{name} is not a generic name", nameof(name));
            genericCount = int.Parse(name.Substring(index + 1));
            return name.Substring(0, index);
        }
    }
}
