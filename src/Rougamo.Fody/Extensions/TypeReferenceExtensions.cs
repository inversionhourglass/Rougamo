using Mono.Cecil;
using System;
using System.Linq;

namespace Rougamo.Fody
{
    internal static class TypeReferenceExtensions
    {
        public static MethodDefinition GetMethod(this TypeReference typeRef, string name, bool recursion) => typeRef.GetMethod(recursion, md => md.Name == name) ?? throw new RougamoException($"Cannot find the method {name} from {typeRef}");

        public static MethodDefinition? GetMethod(this TypeReference typeRef, bool recursion, Func<MethodDefinition, bool> predicate)
        {
            if (typeRef is not TypeDefinition typeDef) typeDef = typeRef.Resolve();

            MethodDefinition? methodDef = null;
            do
            {
                methodDef = typeDef.Methods.SingleOrDefault(predicate);
            } while (methodDef == null && recursion && (typeDef = typeDef.BaseType.Resolve()).FullName != typeof(object).FullName);

            return methodDef;
        }

        public static GenericParameter[] GetSelfGenerics(this TypeDefinition typeDef)
        {
            var declaringTypeDef = typeDef.DeclaringType;
            if (declaringTypeDef == null) return typeDef.GenericParameters.ToArray();

            var declaringTypeGenericNames = declaringTypeDef.GenericParameters.Select(x => x.Name).ToArray();
            return typeDef.GenericParameters.Where(x => !declaringTypeGenericNames.Contains(x.Name)).ToArray();
        }
    }
}
