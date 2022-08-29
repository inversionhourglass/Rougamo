using System;

namespace Rougamo
{
    internal static class Extensions
    {
        public static bool Setable(this Type type, object value)
        {
            if (value == null)
            {
                return !type.IsValueType || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }
            return type.IsAssignableFrom(value.GetType());
        }
    }
}
