using Fody;
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
            weaver.ExecuteTestRun("Rougamo.UsingAssembly.dll");
            //weaver.ExecuteTestRun(@"D:\Code\Test\ConsoleApp1\ClassLibrary2\bin\Debug\netcoreapp3.1\ClassLibrary2.dll");
        }
    }
}
