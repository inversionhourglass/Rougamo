using Fody;
using System;
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
            var weaver = new ModuleWeaver();
            var result = weaver.ExecuteTestRun(assemblyPath, false);
            Assembly = result.Assembly;
        }

        protected dynamic GetInstance(string className, bool fullName = false)
        {
            if (!fullName)
            {
                className = $"{RootNamespace}.{className}";
            }
            var type = Assembly.GetType(className, true);
            return Activator.CreateInstance(type);
        }

        protected dynamic GetStaticInstance(string className, bool fullName = false)
        {
            if (!fullName)
            {
                className = $"{RootNamespace}.{className}";
            }
            var type = Assembly.GetType(className, true);
            return new StaticMembersDynamicWrapper(type);
        }
    }
}
