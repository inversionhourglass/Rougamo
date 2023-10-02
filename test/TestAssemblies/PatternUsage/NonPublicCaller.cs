using Rougamo;
using System;
using System.Reflection;

namespace PatternUsage
{
    public abstract class NonPublicCaller
    {
        [IgnoreMo]
        public object? Call(string methodName, object? executedMos, params Type[] genericTypes)
        {
            var args = executedMos == null ? Array.Empty<object>() : new object[] { executedMos };

            return Call(methodName, args, genericTypes);
        }

        [IgnoreMo]
        public object? Call(string methodName, object[] args, params Type[] genericTypes)
        {
            var method = GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            if (method == null) throw new Exception($"Method({methodName}) not found");

            if (genericTypes.Length > 0) method = method.MakeGenericMethod(genericTypes);

            var caller = method.IsStatic ? null : this;
            return method.Invoke(caller, args);
        }
    }
}
