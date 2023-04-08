using System;
using System.Linq;
using System.Reflection;

namespace Rougamo
{
    internal static class ReflectionExtensions
    {
        public static MethodInfo GetMethod(this Type type, string name, int genericParameterCount, params Type[] parameterTypes)
        {
            return type.GetMethods().Single(x =>
            {
                if (x.Name == name && x.IsGenericMethod && x.GetGenericArguments().Length == genericParameterCount)
                {
                    var parameters = x.GetParameters();
                    if (parameters.Length != parameterTypes.Length) return false;

                    for (var i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType != parameterTypes[i]) return false;
                    }
                    return true;
                }
                return false;
            });
        }
    }
}
