using Rougamo.ImplAssembly;
using System;
using System.Xml.Serialization;

namespace Rougamo.UsingAssembly
{
    public class Class1 : Interface1
    {
        [Abc]
        public void Aha()
        {
            Console.WriteLine();
        }
    }
}
