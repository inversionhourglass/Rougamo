using Fody;
using Rougamo.UsingAssembly;
using System;
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
            var value = class1.InstanceReturnSimple(331);
            Console.WriteLine(value);
            //class1.InstanceVoid("123", new[] { 1, 2, 3 });
            //var testGlobalOnlyInstance = result.Assembly.GetInstance(typeof(TestGlobalOnly).FullName);
            //var testGlobalOnlyClass = result.Assembly.GetStaticInstance(typeof(TestGlobalOnly).FullName);
            //var value = testGlobalOnlyInstance.Instance("456", 123);
            //Console.WriteLine($"value: {value}");
            //testGlobalOnlyClass.Static(new object[] { Guid.NewGuid(), 3345, "okok" }, new Tuple<int, int>(9, 0));
            //weaver.ExecuteTestRun(@"D:\Code\Test\ConsoleApp1\ClassLibrary2\bin\Debug\netcoreapp3.1\ClassLibrary2.dll");
        }
    }
}
