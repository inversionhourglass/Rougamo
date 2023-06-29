using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace Rougamo.Fody.Signature.Patterns
{
    public static class CachedExtensions
    {
        private static readonly Dictionary<MethodDefinition, MethodType> _MethodTypes = new();

        public static bool IsGetter(this MethodDefinition methodDef) => GetMethodType(methodDef) == MethodType.Getter;

        public static bool IsSetter(this MethodDefinition methodDef) => GetMethodType(methodDef) == MethodType.Setter;


        public static bool IsPlainMethod(this MethodDefinition methodDef) => GetMethodType(methodDef) == MethodType.Plain;

        private static MethodType GetMethodType(MethodDefinition methodDef)
        {
            if (!_MethodTypes.TryGetValue(methodDef, out var methodType))
            {
                methodType = MethodType.Plain;
                var methodName = methodDef.Name;
                if (methodDef.DeclaringType != null && !methodDef.HasGenericParameters && methodName.Length > 4)
                {
                    if (methodName.StartsWith("get_"))
                    {
                        var property = methodDef.DeclaringType.Properties.SingleOrDefault(x => x.Name == methodName.Substring(4));
                        methodType = property != null && property.GetMethod == methodDef ? MethodType.Getter : MethodType.Plain;
                    }
                    else if (methodName.StartsWith("set_"))
                    {
                        var property = methodDef.DeclaringType.Properties.SingleOrDefault(x => x.Name == methodName.Substring(4));
                        methodType = property != null && property.SetMethod == methodDef ? MethodType.Setter : MethodType.Plain;
                    }
                }
                _MethodTypes[methodDef] = methodType;
            }

            return methodType;
        }

        enum MethodType
        {
            Plain,
            Getter,
            Setter
        }
    }
}
