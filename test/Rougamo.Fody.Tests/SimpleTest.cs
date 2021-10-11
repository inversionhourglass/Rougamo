using Fody;
using Rougamo.UsingAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xunit;
using static Rougamo.UsingAssembly.Class1;

namespace Rougamo.Fody.Tests
{
    public class SimpleTest
    {
        [Fact]
        public async void Test1()
        {
            var weaver = new ModuleWeaver();
            var result = weaver.ExecuteTestRun("Rougamo.UsingAssembly.dll");
            var class1 = result.Assembly.GetInstance(typeof(Class1).FullName);
            var class1Class = result.Assembly.GetStaticInstance(typeof(Class1).FullName);
            
            class1.AsyncVoid(new Random(), 100);
            try
            {
                class1.AsyncVoidSyncException(Guid.NewGuid());
            }
            catch (ArgumentException)
            {
                // ignore
            }
            class1.AsyncVoidAsyncException(DateTime.Now);
            Thread.Sleep(30000);
            //var enumerable = (IAsyncEnumerable<int>)class1.AE1();
            //await foreach(var item in enumerable)
            //{
            //    Debug.Print(item.ToString());
            //}
            //var enumerable = ((IAsyncEnumerable<int>)class1.AE1()).GetAsyncEnumerator();
            //while (enumerable.MoveNextAsync().AsTask().Result)
            //{
            //    Debug.Print(enumerable.Current.ToString());
            //}
            //class1.MultiAwait().Wait();
            //class1.HttpAsync().Wait();
            //class1.HttpValueAsync().AsTask().Wait();
            //try
            //{
            //    class1.Thrown();
            //}
            //catch
            //{

            //}
            //class1.Empty();
            //class1Class.Sync();

            #region unsucess
            //byte a = default;
            //sbyte b = default;
            //short c = default;
            //ushort d = default;
            //int e = default;
            //uint f = default;
            //long g = default;
            //ulong h = default;
            //float i = default;
            //double j = default;
            //decimal v = default;
            //Enu[] enu = new Enu[0];
            //Ab ab = new Ab();
            //Class1 cls = new Class1();
            //class1.Test(ref a, out b, ref c, out d, ref e, ref f, ref g, out h, ref i, ref j, ref v, ref enu, ref ab, ref cls);
            #endregion
            //class1.InstanceVoid("123", new[] { 1, 2, 3 });
        }
    }
}
