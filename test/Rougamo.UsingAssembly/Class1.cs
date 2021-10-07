using Rougamo.Context;
using Rougamo.ImplAssembly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rougamo.UsingAssembly
{
    public class Class1
    {
        public enum Enu : byte
        {
            A,
            B
        }

        public struct Ab
        {
            public int Id { get; set; }
        }
        private IMo[] _mos;

        private static void SyncException()
        {
            Console.WriteLine("SyncException");
            throw new Exception();
        }

        public static void Sync()
        {
            Console.WriteLine(1);
            try
            {
                Console.WriteLine(2);
                throw new ArgumentNullException();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task HttpAsync()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://www.baidu.com").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Console.WriteLine(content);
        }

        public async ValueTask HttpValueAsync()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://www.baidu.com").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Console.WriteLine(content);
        }

        public async Task MultiAwait()
        {
            Console.WriteLine(1);
            await Task.Delay(1000);
            Console.WriteLine(2);
            await Task.Delay(1000);
            Console.WriteLine(3);
        }

        public IEnumerable<string> E1()
        {
            var random = new Random();
            var start = random.Next(33, 126 / 2);
            var count = random.Next(1, 126 - start);
            for (int i = 0; i < count; i++)
            {
                if (i % 6 == 0) throw new InvalidOperationException("666");
                yield return ((char)(start + i)).ToString();
            }
        }

        public string Thrown()
        {
            throw new Exception();
        }

        public void Empty()
        {

        }

        //public static void Abc()
        //{
        //    IMo attr = new StaticMoAttribute();
        //    var context = new MethodContext(null, typeof(Class1), null, new object[0]);
        //    attr.OnEntry(context);
        //    try
        //    {
        //        Console.WriteLine("SyncException");
        //        throw new Exception();
        //    }
        //    catch (Exception e)
        //    {
        //        context.Exception = e;
        //        attr.OnException(context);
        //        throw;
        //    }
        //    finally
        //    {
        //        if (!context.HasException)
        //        {
        //            attr.OnSuccess(context);
        //        }
        //        attr.OnExit(context);
        //    }
        //}

        //public void Test(ref byte a, out sbyte b, ref short c, out ushort d, ref int e, ref uint f, ref long g, out ulong h, ref float i, ref double j, ref decimal v, ref Enu[] enu, ref Ab ab, ref Class1 cls)
        //{
        //    a = default;
        //    b = default;
        //    c = default;
        //    d = default;
        //    e = default;
        //    f = default;
        //    g = default;
        //    h = default;
        //    i = default;
        //    j = default;
        //    v = default;
        //    enu = default;
        //    ab = default;
        //    cls = default;
        //    var obj1 = (object)v;
        //    var obj2 = (object)ab;
        //    Console.WriteLine(a);
        //    Console.WriteLine(b);
        //    Console.WriteLine(c);
        //    Console.WriteLine(d);
        //    Console.WriteLine(e);
        //    Console.WriteLine(f);
        //    Console.WriteLine(g);
        //    Console.WriteLine(h);
        //    Console.WriteLine(i);
        //    Console.WriteLine(j);
        //    Console.WriteLine(enu);
        //}

        //public void ForEach()
        //{
        //try
        //{
        //    Console.WriteLine(1);
        //}
        //catch (Exception e)
        //{
        //    Console.WriteLine(e);
        //    for (int i = 0; i < _mos.Length; i++)
        //    {
        //        _mos[i].OnEntry(null);
        //    }
        //    Console.WriteLine(3);
        //}
        //Console.WriteLine();
        //foreach (var item in _mos)
        //{
        //    item.OnEntry(null);
        //}
        //}

        //public async Task<int> InstanceVoid(string stringValue, int[] intArray)
        //{
        //    Console.WriteLine("before");
        //    Console.WriteLine(this._mos);
        //    await Task.Delay(100);
        //    Console.WriteLine("middle");
        //    await Task.Delay(100);
        //    throw new Exception();
        //    Console.WriteLine("after");
        //    return 123;

        //}

        //public int InstanceReturn(int seed)
        //{
        //    var random = new Random(seed);
        //    for (int i = 0; i < 100; i++)
        //    {
        //        if (random.Next() == 121) return 123;
        //    }

        //    try
        //    {
        //        var x = 0;
        //        var y = 0;
        //        do
        //        {
        //            y += random.Next();
        //            if (y % 157 == 0) return y + 715;
        //            if (y % 715 == 0) throw new Exception();
        //        } while (x++ < 100);
        //        return 731;
        //    }
        //    catch (Exception e)
        //    {
        //        if (random.Next() % 31 == 2) return 31;
        //        throw e;
        //    }
        //}

        //public int InstanceReturnSimple(int seed)
        //{
        //    var arr = new int[] { 1, 2, 3 };
        //    var random = new Random(seed);
        //    var count = 0;
        //    while (count++ < 100)
        //    {
        //        var rdv = random.Next();
        //        if (rdv % 101 == 123)
        //        {
        //            seed = rdv;
        //            break;
        //        }
        //    }
        //    return arr[0];
        //}

        //public static void StaticVoid(int k, int p)
        //{
        //    var random = new Random(k | p);
        //    var i = 0;
        //    try
        //    {
        //        for (int j = 0; j < 15; j++)
        //        {
        //            i += random.Next(17, 347);
        //        }
        //        if (i < 345) throw new UnluckilyException();
        //        if (i > 123456) Console.WriteLine("success"); ;
        //    }
        //    catch (IOException e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //}

        //public static int StaticReturn()
        //{
        //start:
        //    var random = new Random();
        //    for (int i = 0; i < 100; i++)
        //    {
        //        if (random.Next() == 121) return 123;
        //    }

        //    try
        //    {
        //        var x = 0;
        //        var y = 0;
        //        do
        //        {
        //            y += random.Next();
        //            if (y % 157 == 0) return y + 715;
        //            if (y % 715 == 0) throw new UnluckilyException();
        //            if (y % 332 == 12) goto start;
        //        } while (x++ < 100);
        //        return 731;
        //    }
        //    catch (Exception e)
        //    {
        //        if (random.Next() % 31 == 2) return 31;
        //        throw e;
        //    }
        //}
    }
}
