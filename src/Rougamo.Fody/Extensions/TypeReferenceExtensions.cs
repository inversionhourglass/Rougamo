using Mono.Cecil;
using System;
using System.Collections.Generic;
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
            if (typeDef.IsInterface)
            {
                methodDef = typeDef.Methods.SingleOrDefault(predicate);
                if (methodDef == null)
                {
                    foreach (var @interface in typeDef.Interfaces)
                    {
                        methodDef = @interface.InterfaceType.Resolve().Methods.SingleOrDefault(predicate);
                        if (methodDef != null) break;
                    }
                }

                return methodDef;
            }

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

        public static TypeDefinition AddUniqueField(this TypeDefinition typeDef, FieldDefinition? fieldDef)
        {
            if (fieldDef == null) return typeDef;

            var fields = typeDef.Fields;
            var currentFieldDef = fields.FirstOrDefault(x => x.Name == fieldDef.Name);
            if (currentFieldDef != null) fields.Remove(currentFieldDef);
            fields.Add(fieldDef);

            return typeDef;
        }

        public static Dictionary<string, TypeReference> GetGenericMap(this TypeReference typeRef)
        {
            if (typeRef is GenericParameter gp) return new() { { gp.Name, gp } };
            if (typeRef is TypeDefinition td) return td.GenericParameters.ToDictionary(x => x.Name, x => (TypeReference)x);
            if (typeRef is not GenericInstanceType git) return [];

            var typeDef = typeRef.Resolve();
            var map = new Dictionary<string, TypeReference>();

            for (var i = 0; i < typeDef.GenericParameters.Count; i++)
            {
                map[typeDef.GenericParameters[i].Name] = git.GenericArguments[i];
            }

            return map;
        }

        public static Dictionary<string, TypeReference> GetGenericMap(this MethodDefinition methodDef)
        {
            var map = methodDef.DeclaringType.GetGenericMap();

            foreach (var gp in methodDef.GenericParameters)
            {
                map[gp.Name] = gp;
            }

            return map;
        }
    }
}
