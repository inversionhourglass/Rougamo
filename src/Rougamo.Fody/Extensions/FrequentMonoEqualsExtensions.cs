using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace Rougamo.Fody
{
    internal static class FrequentMonoEqualsExtensions
    {
        public static bool IsVoid(this TypeReference typeReference)
        {
            return typeReference.Is(Constants.TYPE_Void);
        }

        public static bool IsTask(this TypeReference typeReference)
        {
            return typeReference.Is(Constants.TYPE_Task);
        }

        public static bool IsGeneric(this TypeReference typeRef, string typeFullName, int genericCount)
        {
            if (typeRef is GenericInstanceType git)
            {
                return git.GenericArguments.Count == genericCount && git.ElementType.FullName == $"{typeFullName}`{genericCount}";
            }
            if (typeRef is TypeDefinition typeDef && typeDef.HasGenericParameters)
            {
                return typeDef.GenericParameters.Count == genericCount && typeDef.FullName == $"{typeFullName}`{genericCount}";
            }
            return false;
        }

        public static bool IsGenericTask(this TypeReference typeReference) => IsGeneric(typeReference, Constants.TYPE_Task, 1);

        public static bool IsValueTask(this TypeReference typeReference)
        {
            return typeReference.Is(Constants.TYPE_ValueTask);
        }

        public static bool IsGenericValueTask(this TypeReference typeReference)
        {
            var fullName = typeReference.Resolve()?.FullName;

            if (fullName == null) return false;

            return fullName.StartsWith(Constants.TYPE_ValueTask) && fullName.Length != Constants.TYPE_ValueTask.Length;
        }

        public static bool IsAsyncMethodType(this TypeReference typeRef)
        {
            var typeName = typeRef.FullName;
            if (typeName == Constants.TYPE_Task || typeName == Constants.TYPE_ValueTask || typeName.StartsWith($"{Constants.TYPE_Task}`1") || typeName.StartsWith($"{Constants.TYPE_ValueTask}`1")) return true;

            var typeDef = typeRef.Resolve();
            return typeDef != null && typeDef.CustomAttributes.Any(x => x.Is(Constants.TYPE_AsyncMethodBuilder));
        }

        public static bool IsLdarg(this OpCode opCode)
        {
            var code = opCode.Code;
            return new[] { Code.Ldarg, Code.Ldarg_S, Code.Ldarg_0, Code.Ldarg_1, Code.Ldarg_2, Code.Ldarg_3 }.Contains(code);
        }
    }
}
