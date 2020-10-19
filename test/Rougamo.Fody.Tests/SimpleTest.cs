using Fody;
using Rougamo.UsingAssembly;
using System;
using System.Threading;
using Xunit;

namespace Rougamo.Fody.Tests
{
    public class SimpleTest
    {
        [Fact]
        public void Test1()
        {
            var weaver = new ModuleWeaver();
            var result = weaver.ExecuteTestRun("Rougamo.UsingAssembly.dll");
            var class1 = result.Assembly.GetInstance(typeof(Class1).FullName);
            var class1Class = result.Assembly.GetStaticInstance(typeof(Class1).FullName);
            var value = class1.InstanceReturnSimple(331);
            Console.WriteLine(value);
            class1.InstanceVoid("123", new[] { 1, 2, 3 });
        }
    }
}
