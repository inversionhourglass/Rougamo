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
            var genericParameters = new List<GenericParameterItem>();
            var modifier = ParseModifier(methodDef);
            var declareType = ParseType(methodDef.DeclaringType, genericParameters, null);
            var typeGenericCount = genericParameters.Count;
            var methodName = methodDef.Name;
            var methodGenericParameters = ParseMethodGenericParameters(methodDef, genericParameters);
            var genericNameMap = SortGenericParameter(typeGenericCount, genericParameters);
            var returnType = ParseType(methodDef.ReturnType, null, genericNameMap);
            var parameters = ParseMethodParameters(methodDef, genericNameMap);
            return new MethodSignature(modifier, returnType, declareType, methodName, methodGenericParameters, parameters);
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

        public static TypeSignature ParseType(TypeReference typeRef, List<GenericParameterItem>? genericParameters, Dictionary<string, string>? genericNameMap)
        {
            if (typeRef is GenericParameter gp)
            {
                var signature = new GenericParameterTypeSignature(typeRef.Name, typeRef);
                if (genericParameters != null)
                {
                    genericParameters.Add(new GenericParameterItem(gp, signature));
                }
                if (genericNameMap != null && genericNameMap.TryGetValue(typeRef.Name, out var sortName))
                {
                    signature.SortName = sortName;
                }
                return signature;
            }

            if (typeRef is GenericInstanceType git)
            {
                var name = ShortGenericName(typeRef.Name, out var genericCount);
                var argumentSignatures = new List<TypeSignature>();
                for (var i = git.GenericArguments.Count - 1; i >= git.GenericArguments.Count - genericCount; i--)
                {
                    var argumentSignature = ParseType(git.GenericArguments[i], genericParameters, genericNameMap);
                    argumentSignatures.Add(argumentSignature!);
                }
                return new GenericTypeSignature(name, ParseDeclaringType(typeRef, genericParameters, genericNameMap), argumentSignatures.AsEnumerable().Reverse().ToArray(), typeRef);
            }

            if (typeRef.HasGenericParameters)
            {
                var name = ShortGenericName(typeRef.Name, out var genericCount);
                var parameterSignatures = new List<TypeSignature>();
                for (var i = typeRef.GenericParameters.Count - 1; i >= typeRef.GenericParameters.Count - genericCount; i--)
                {
                    var parameterSignature = ParseType(typeRef.GenericParameters[i], genericParameters, genericNameMap);
                    parameterSignatures.Add(parameterSignature!);
                }
                return new GenericTypeSignature(name, ParseDeclaringType(typeRef, genericParameters, genericNameMap), parameterSignatures.AsEnumerable().Reverse().ToArray(), typeRef);
            }

            return new SimpleTypeSignature(typeRef.Name, ParseDeclaringType(typeRef, genericParameters, genericNameMap), typeRef);
        }

        private static TypeSignature ParseDeclaringType(TypeReference typeRef, List<GenericParameterItem>? genericParameters, Dictionary<string, string>? genericNameMap)
        {
            return typeRef.DeclaringType == null ? new NamespaceSignature(typeRef.Namespace) : ParseType(typeRef.DeclaringType, genericParameters, genericNameMap);
        }

        private static TypeSignature[] ParseMethodGenericParameters(MethodDefinition methodDef, List<GenericParameterItem> genericParameters)
        {
            return methodDef.HasGenericParameters ? methodDef.GenericParameters.Select(x => ParseType(x, genericParameters, null)!).ToArray() : new TypeSignature[0];
        }

        private static TypeSignature[] ParseMethodParameters(MethodDefinition methodDef, Dictionary<string, string>? genericNameMap)
        {
            return methodDef.HasParameters ? methodDef.Parameters.Select(x => ParseType(x.ParameterType, null, genericNameMap)!).ToArray() : new TypeSignature[0];
        }

        private static Dictionary<string, string>? SortGenericParameter(int typeGenericCount, List<GenericParameterItem> genericParameters)
        {
            if (genericParameters.Count == 0) return null;
            if (genericParameters.Count == 1)
            {
                var genericParameter = genericParameters[0];
                genericParameter.Signature.SortName = "T";
                return new Dictionary<string, string> { { genericParameter.Parameter.Name, genericParameter.Signature.SortName } };
            }
            for (var i = 0; i < genericParameters.Count; i++)
            {
                if (i < typeGenericCount)
                {
                    genericParameters[i].Signature.SortName = $"T{typeGenericCount - i}";
                }
                else
                {
                    genericParameters[i].Signature.SortName = $"T{i + 1}";
                }
            }
            return genericParameters.ToDictionary(x => x.Parameter.Name, x => x.Signature.SortName);
        }

        private static string ShortGenericName(string name, out int genericCount)
        {
            var index = name.IndexOf('`');
            if (index == -1) throw new ArgumentException($"{name} is not a generic name", nameof(name));
            genericCount = int.Parse(name.Substring(index + 1));
            return name.Substring(0, index);
        }

        public struct GenericParameterItem
        {
            public GenericParameterItem(GenericParameter parameter, GenericParameterTypeSignature signature)
            {
                Parameter = parameter;
                Signature = signature;
            }

            public GenericParameter Parameter { get; }

            public GenericParameterTypeSignature Signature { get; }
        }
    }
}
