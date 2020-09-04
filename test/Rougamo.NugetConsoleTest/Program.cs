using Rougamo.NugetUsingAssembly;
using System;

namespace Rougamo.NugetConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var class1 = new Class1();
            class1.InstanceVoid("instanceVoidArg1", new[] { 1, 2, 3, 4, 5, 999 });
            class1.InstanceReturn(331);
            Class1.StaticVoid(7, 11);
            Class1.StaticReturn();
        }
    }
}
