using Rougamo.Context;
using Rougamo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace IndirectDependency2
{
    internal static class SetOnEntryExtensions
    {
        private const string LIST_KEY = nameof(LIST_KEY);
        private const string MAP_KEY = nameof(MAP_KEY);

        public static void SetOnEntry(this IMo mo, MethodContext context)
        {
            var name = mo.GetType().FullName;

            if (context.Arguments.Length >= 1)
            {
                var arg = context.Arguments[0];
                if (arg is List<string> executedMos)
                {
                    executedMos.Add(name);
                    return;
                }
                if (arg is IDictionary map)
                {
                    map[name] = name;
                    return;
                }
            }
            if (context.Method is MethodInfo mi)
            {
                var returnType = mi.ReturnType;
                if (returnType.IsGenericType && (returnType.GetGenericTypeDefinition() == typeof(Task<>) || returnType.GetGenericTypeDefinition() == typeof(ValueTask<>)))
                {
                    returnType = returnType.GetGenericArguments()[0];
                }
                if (returnType == typeof(List<string>))
                {
                    if (!context.Datas.Contains(LIST_KEY) || context.Datas[LIST_KEY] is not List<string> mos)
                    {
                        mos = new List<string>();
                        context.Datas[LIST_KEY] = mos;
                    }
                    mos.Add(name);
                    context.ReplaceReturnValue(mo, mos);
                }
                else if (typeof(IDictionary).IsAssignableFrom(returnType))
                {
                    if (!context.Datas.Contains(MAP_KEY) || context.Datas[MAP_KEY] is not IDictionary map)
                    {
                        map = (IDictionary)Activator.CreateInstance(returnType);
                        context.Datas[MAP_KEY] = map;
                    }
                    map[name] = name;
                    context.ReplaceReturnValue(mo, map);
                }
            }
        }
    }
}
