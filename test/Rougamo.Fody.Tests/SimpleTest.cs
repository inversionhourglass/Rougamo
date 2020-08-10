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
        }
    }
}
