using Fody;
using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Rougamo.Fody.Tests
{
    public class WeavedAssembly
    {
        public WeavedAssembly(string defaultNamespace) : this(defaultNamespace, null) { }

        public WeavedAssembly(string defaultNamespace, string config) : this($"{defaultNamespace}.dll", defaultNamespace, config) { }

        public WeavedAssembly(string assemblyName, string defaultNamespace, string? config)
        {
            DefaultNamespace = defaultNamespace;
            WeaveAssembly(assemblyName, config);
        }

        public string DefaultNamespace { get; }

        public Assembly Assembly { get; private set; }

        private void WeaveAssembly(string assemblyPath, string? config)
        {
            var weaver = new ModuleWeaver(true);
            if (!string.IsNullOrEmpty(config))
            {
                weaver.Config = XElement.Parse(config);
            }
            var result = weaver.ExecuteTestRun(assemblyPath, false);
            Assembly = result.Assembly;
        }

        public dynamic GetInstance(string className, bool fullName = false, Func<Type, Type>? processor = null, object[]? args = null)
        {
            if (!fullName)
            {
                className = $"{DefaultNamespace}.{className}";
            }
            var type = Assembly.GetType(className, true)!;
            if (processor != null) type = processor(type);

            return Activator.CreateInstance(type, args)!;
        }

        public dynamic GetStaticInstance(string className, bool fullName = false, Func<Type, Type>? processor = null)
        {
            if (!fullName)
            {
                className = $"{DefaultNamespace}.{className}";
            }
            var type = Assembly.GetType(className, true)!;
            if (processor != null) type = processor(type);

            return new StaticMembersDynamicWrapper(type);
        }

        public T GetMethodDelegate<T>(object instance, string methodName) where T : Delegate
        {
            var method = instance.GetType().GetMethods().Single(x => x.Name == methodName);

            return (T)method.CreateDelegate(typeof(T), instance);
        }

        public Type? GetType(string fullName)
        {
            return Assembly.GetType(fullName);
        }
    }
}
