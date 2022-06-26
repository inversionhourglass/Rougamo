using Mono.Cecil;

namespace Rougamo.Fody
{
    internal static class FrequentMonoEqualsExtensions
    {
        public static bool IsVoid(this TypeReference typedReference)
        {
            return typedReference.Is(Constants.TYPE_Void);
        }
    }
}
