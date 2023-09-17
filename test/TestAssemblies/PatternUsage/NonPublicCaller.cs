using Rougamo;
using System;
using System.Reflection;

namespace PatternUsage
{
    public abstract class NonPublicCaller
    {
        [IgnoreMo]
        public object? Call(string methodName, object? executedMos)
        {
            var method = GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            if (method == null) throw new Exception($"Method({methodName}) not found");

            var caller = method.IsStatic ? null : this;
            var args = executedMos == null ? new object[0] : new object[] { executedMos };
            return method.Invoke(caller, args);
        }

        [IgnoreMo]
        public object? Call(string methodName) => Call(methodName, null);
    }
}
