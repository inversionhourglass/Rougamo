using Rougamo.UsingAssembly;
using System;

namespace Rougamo.FileReferenceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var cs = new Class1();
            Console.WriteLine(cs.InstanceReturnSimple(123));
        }
    }
}
