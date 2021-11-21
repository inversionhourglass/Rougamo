using Rougamo.ImplAssembly;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: Mo2]
[assembly: Mo5]
namespace Rougamo.UsingAssembly
{
    public class Class2 : IRepulsionsRougamo<Mo1Attribute, TestRepulsion>
    {
        [Mo3]
        public void M1()
        {
            Console.WriteLine("m1");
        }

        [Mo4]
        public void M2()
        {
            Console.WriteLine("m2");
        }
    }
}
