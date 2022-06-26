using Mono.Cecil;

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

        public static bool IsGenericTask(this TypeReference typeReference)
        {
            var fullName = typeReference.Resolve()?.FullName;

            if (fullName == null) return false;

            return fullName.StartsWith(Constants.TYPE_Task) && fullName.Length != Constants.TYPE_Task.Length;
        }

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
    }
}
