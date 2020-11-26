using Fody;
using Rougamo.UsingAssembly;
using System;
using System.Threading;
using Xunit;
using static Rougamo.UsingAssembly.Class1;

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
            byte a = 1;
            sbyte b = 2;
            short c = 3;
            ushort d = 4;
            int e = 5;
            uint f = 6;
            long g = 7;
            ulong h = 8;
            float i = 9.9f;
            double j = 10.01;
            Enu enu = Enu.A;
            class1Class.SyncException("1", 2);
            //class1.Test(ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h, ref i, ref j, ref enu);
            //class1.InstanceVoid("123", new[] { 1, 2, 3 });
        }
    }
}
