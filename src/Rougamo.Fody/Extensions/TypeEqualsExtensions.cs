using Mono.Cecil;
using System.Linq;

namespace Rougamo.Fody
{
    internal static class TypeEqualsExtensions
    {
        public static bool Is(this TypeReference typeRef, string fullName)
        {
            if (typeRef is not TypeDefinition typeDef)
            {
                typeDef = typeRef.Resolve();
            }
            return typeDef != null && typeDef.FullName == fullName;
        }

        public static bool Is(this CustomAttribute attribute, string fullName)
        {
            return attribute.AttributeType.Is(fullName);
        }

        public static bool IsGeneric(this TypeReference typeRef, string typeFullName, int genericCount)
        {
            return typeRef.IsGeneric(typeFullName, genericCount, out _);
        }

        public static bool IsGeneric(this TypeReference typeRef, string typeFullName, int genericCount, out TypeReference[]? genericTypeRefs)
        {
            if (typeRef is GenericInstanceType git)
            {
                if (git.GenericArguments.Count == genericCount && git.ElementType.FullName == $"{typeFullName}`{genericCount}")
                {
                    genericTypeRefs = git.GenericArguments.ToArray();
                    return true;
                }
            }
            if (typeRef is TypeDefinition typeDef && typeDef.HasGenericParameters)
            {
                if (typeDef.GenericParameters.Count == genericCount && typeDef.FullName == $"{typeFullName}`{genericCount}")
                {
                    genericTypeRefs = typeDef.GenericParameters.ToArray();
                    return true;
                }
            }
            genericTypeRefs = null;
            return false;
        }

        public static bool Implement(this TypeReference typeRef, string @interface)
        {
            if (typeRef is not TypeDefinition typeDef)
            {
                typeDef = typeRef.Resolve();
            }

            if (typeDef == null) return false;

            if (typeDef.Interfaces.Any(x => x.InterfaceType.FullName == @interface)) return true;

            return typeDef.BaseType != null && typeDef.BaseType.Implement(@interface);
        }

        public static bool Inherit(this TypeReference typeRef, string baseClass)
        {
            if (typeRef is not TypeDefinition typeDef)
            {
                typeDef = typeRef.Resolve();
            }
            if (typeDef == null) return false;

            var baseTypeRef = typeDef.BaseType;
            if (baseTypeRef == null) return false;
            
            if (baseTypeRef.FullName == baseClass) return true;

            return baseTypeRef.Inherit(baseClass);
        }

        public static bool InheritAny(this TypeReference typeRef, params string[] baseClasses)
        {
            foreach (var baseClass in baseClasses)
            {
                if (typeRef.Inherit(baseClass)) return true;
            }

            return false;
        }

        public static bool IsOrInherit(this TypeReference typeRef, string className)
        {
            return typeRef.Is(className) || typeRef.Inherit(className);
        }

        #region Fast Check

        public static bool IsObject(this TypeReference typeRef) => typeRef.Is(typeof(object).FullName);

        public static bool IsVoid(this TypeReference typeRef)
        {
            return typeRef.Is(Constants.TYPE_Void);
        }

        public static bool IsString(this TypeReference typeRef)
        {
            return typeRef.Resolve().FullName == typeof(string).FullName;
        }

        public static bool IsTask(this TypeReference typeRef)
        {
            return typeRef.Is(Constants.TYPE_Task);
        }

        public static bool IsGenericTask(this TypeReference typeRef) => typeRef.IsGeneric(Constants.TYPE_Task, 1);

        public static bool IsValueTask(this TypeReference typeRef)
        {
            return typeRef.Is(Constants.TYPE_ValueTask);
        }

        public static bool IsGenericValueTask(this TypeReference typeRef) => typeRef.IsGeneric(Constants.TYPE_ValueTask, 1);

        public static bool IsDelegate(this TypeReference typeRef)
        {
            return typeRef.Inherit(Constants.TYPE_MulticastDelegate);
        }

        public static bool IsArray(this TypeReference typeRef, out TypeReference? elementType)
        {
            elementType = null;
            if (typeRef is not ArrayType at) return false;

            elementType = at.ElementType;
            return true;
        }

        public static bool IsEnum(this TypeReference typeRef)
        {
            if (typeRef is not TypeDefinition typeDef)
            {
                typeDef = typeRef.Resolve();
            }
            return typeDef != null && typeDef.IsEnum;
        }

        public static bool IsEnum(this TypeReference typeRef, out TypeReference? underlyingType)
        {
            if (typeRef is not TypeDefinition typeDef)
            {
                typeDef = typeRef.Resolve();
            }
            if (typeDef != null && typeDef.IsEnum)
            {
                underlyingType = typeDef.Fields.First(f => f.Name == "value__").FieldType;
                return true;
            }

            underlyingType = null;
            return false;
        }

        #endregion Fast Check
    }
}
