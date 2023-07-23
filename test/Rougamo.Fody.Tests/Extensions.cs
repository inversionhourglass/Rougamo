using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace Rougamo.Fody.Tests
{
    public static class Extensions
    {
        public static string[] GetInterfaceBaseTypes(this TypeReference typeRef)
        {
            var typeDef = typeRef.Resolve();
            if (typeDef == null) return Array.Empty<string>();

            var list = new List<string>();

            foreach (var item in typeDef.Interfaces)
            {
                list.Add(item.InterfaceType.FullName);
            }

            do
            {
                typeDef = typeRef.Resolve();
                list.Add(typeDef.FullName);
            } while ((typeRef = typeDef.BaseType) != null);

            return list.ToArray();
        }
    }
}
