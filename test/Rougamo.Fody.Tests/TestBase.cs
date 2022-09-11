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

        protected dynamic GetInstance(string className)
        {
            var type = Assembly.GetType($"{RootNamespace}.{className}", true);
            return Activator.CreateInstance(type);
        }

        protected dynamic GetStaticInstance(string className)
        {
            var type = Assembly.GetType($"{RootNamespace}.{className}", true);
            return new StaticMembersDynamicWrapper(type);
        }
    }
}
