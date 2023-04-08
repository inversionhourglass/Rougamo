using System.Linq;

namespace System.Reflection
{
    /// <summary>
    /// Relection extensions
    /// </summary>
    public static class PublicReflectionExtensions
    {
        private const BindingFlags _ALL = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// <paramref name="method"/> is property getter method or setter method
        /// </summary>
        public static bool IsProperty(this MethodInfo method) => PropertyMatch(method, p => p.GetMethod == method || p.SetMethod == method);

        /// <summary>
        /// <paramref name="method"/> is property getter method
        /// </summary>
        public static bool IsPropertyGetter(this MethodInfo method) => PropertyMatch(method, p => p.GetMethod == method);

        /// <summary>
        /// <paramref name="method"/> is property setter method
        /// </summary>
        public static bool IsPropertySetter(this MethodInfo method) => PropertyMatch(method, p => p.SetMethod == method);

        private static bool PropertyMatch(this MethodInfo method, Func<PropertyInfo, bool> propertyPredicate)
        {
            if (method.DeclaringType == null) return false;

            return method.DeclaringType.GetProperties(_ALL).Any(propertyPredicate);
        }
    }
}
