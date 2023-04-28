using Mono.Cecil;
using System;
using System.Linq;

namespace Rougamo.Fody.Signature
{
    public class SignatureParser
    {
        public static MethodSignature ParseMethod(MethodDefinition methodDef)
        {
            var modifier = ParseModifier(methodDef.Attributes);
            var returnType = ParseType(methodDef.ReturnType.Resolve());
            var declareType = ParseType(methodDef.DeclaringType.Resolve());
            var methodName = methodDef.Name;
            var methodGenericParameters = ParseMethodGenericParameters(methodDef);
            var parameters = ParseMethodParameters(methodDef);
            return new MethodSignature(modifier, returnType, declareType, methodName, methodGenericParameters, parameters);
        }

        public static TypeSignature ParseType(TypeDefinition typeDef)
        {
            if (typeDef.IsGenericParameter) return new TypeSignature(typeDef.Name, TypeCategory.GenericParameter);
            if (!typeDef.HasGenericParameters) return new TypeSignature(typeDef.FullName);
            return null;
        }

        private static Modifier ParseModifier(MethodAttributes attribute)
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
                return modifier | Modifier.Protected | Modifier.Internal;
            }
            if ((attribute & MethodAttributes.Family) != 0)
            {
                return modifier | Modifier.Protected;
            }
            if ((attribute & MethodAttributes.FamANDAssem) != 0)
            {
                return modifier | Modifier.Private | Modifier.Protected;
            }
            if ((attribute & MethodAttributes.Private) != 0)
            {
                return modifier | Modifier.Private;
            }

            throw new ArgumentException($"Unable parse {attribute} to Modifier", nameof(attribute));
        }

        private static string[] ParseMethodGenericParameters(MethodDefinition methodDef)
        {
            return methodDef.HasGenericParameters? methodDef.GenericParameters.Select(x => x.Name).ToArray() : new string[0];
        }

        private static TypeSignature[] ParseMethodParameters(MethodDefinition methodDef)
        {
            if (!methodDef.HasParameters) return new TypeSignature[0];

            return methodDef.Parameters.Select(x => ParseType(x.ParameterType.Resolve())).ToArray();
        }
    }
}
