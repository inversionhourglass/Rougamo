using System;
using System.Collections.Generic;
using System.Text;

namespace Rougamo.UsingAssembly
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class AAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class BAttribute : Attribute
    {
    }

    [A, B]
    public class Test1
    {

    }

    public class Test2 : Test1
    {

    }
}
