using System;
using System.Reflection;

namespace Rougamo.Fody.Tests
{
    internal static class Extensions
    {
        public static dynamic GetInstance(this Assembly assembly, string className)
        {
            var type = assembly.GetType(className, true);
            return Activator.CreateInstance(type);
        }

        public static dynamic GetStaticInstance(this Assembly assembly, string className)
        {
            var type = assembly.GetType(className, true);
            return new StaticMembersDynamicWrapper(type);
        }
    }
}
