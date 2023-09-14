using Rougamo;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PatternUsage
{
    public abstract class NonPublicCaller
    {
        [IgnoreMo]
        public object? Call(string methodName, List<string> executedMos)
        {
            var method = GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            if (method == null) throw new Exception($"Method({methodName}) not found");

            var caller = method.IsStatic ? null : this;
            return method.Invoke(caller, new object[] { executedMos });
        }
    }
}
