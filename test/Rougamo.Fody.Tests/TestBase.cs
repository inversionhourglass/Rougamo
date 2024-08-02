using Fody;
using System;
using System.Linq;
using System.Reflection;

namespace Rougamo.Fody.Tests
{
    public abstract class TestBase
    {
        protected TestBase(string assemblyName)
        {
            WeaveAssembly(assemblyName);
        }

        protected abstract string RootNamespace { get; }

        protected Assembly Assembly { get; private set; }

        protected void WeaveAssembly(string assemblyPath)
        {
            var weaver = new ModuleWeaver(true);
            var result = weaver.ExecuteTestRun(assemblyPath, false);
            Assembly = result.Assembly;
        }

        protected dynamic GetInstance(string className, bool fullName = false, Func<Type, Type> processor = null, object[] args = null)
        {
            if (!fullName)
            {
                className = $"{RootNamespace}.{className}";
            }
            var type = Assembly.GetType(className, true);
            if (processor != null) type = processor(type);

            return Activator.CreateInstance(type, args);
        }

        protected dynamic GetStaticInstance(string className, bool fullName = false, Func<Type, Type> processor = null)
        {
            if (!fullName)
            {
                className = $"{RootNamespace}.{className}";
            }
            var type = Assembly.GetType(className, true);
            if (processor != null) type = processor(type);

            return new StaticMembersDynamicWrapper(type);
        }

        protected T GetMethodDelegate<T>(object instance, string methodName) where T : Delegate
        {
            var method = instance.GetType().GetMethods().Single(x => x.Name == methodName);

            return (T)method.CreateDelegate(typeof(T), instance);
        }
    }
}
