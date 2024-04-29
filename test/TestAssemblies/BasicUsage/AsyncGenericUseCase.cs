using BasicUsage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class Proxy_AsyncGenericUseCase<T>
    {
        [Async]
        public async Task<T> First(List<T> items)
        {
            await Task.Yield();
            return items.FirstOrDefault();
        }

        [Async]
        public async ValueTask<(T, TT)> Last<TT>(List<T> list1, List<TT> list2)
        {
            await Task.Yield();
            return (list1.Last(), list2.Last());
        }
    }

    [Async]
    public class Proxy_Cls
    {
        public async Task<int> M1(string s1, string s2)
        {
            await Console.Out.WriteLineAsync(s1 + s2);
            return 123;
        }

        public async ValueTask<int> M2(string s1, string s2)
        {
            await Console.Out.WriteLineAsync(s1 + s2);
            return 123;
        }
        public static async Task<int> M3(string s1, string s2)
        {
            await Console.Out.WriteLineAsync(s1 + s2);
            return 123;
        }

        public static async ValueTask<int> M4(string s1, string s2)
        {
            await Console.Out.WriteLineAsync(s1 + s2);
            return 123;
        }
    }

    [Async]
    public struct Proxy_St
    {
        public async Task<int> M1(string s1, string s2)
        {
            await Console.Out.WriteLineAsync(s1 + s2);
            return 123;
        }

        public async ValueTask<int> M2(string s1, string s2)
        {
            await Console.Out.WriteLineAsync(s1 + s2);
            return 123;
        }
        public static async Task<int> M3(string s1, string s2)
        {
            await Console.Out.WriteLineAsync(s1 + s2);
            return 123;
        }

        public static async ValueTask<int> M4(string s1, string s2)
        {
            await Console.Out.WriteLineAsync(s1 + s2);
            return 123;
        }
    }

    [Async]
    public class Proxy_Class53<T1, T2>
    {
        public async Task<int> M1(T1 s1, T2 s2)
        {
            await Task.Yield();
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            return 123;
        }

        public async ValueTask<int> M2(T1 s1, T2 s2)
        {
            await Task.Yield();
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            return 123;
        }
        public static async Task<int> M3(T1 s1, T2 s2)
        {
            await Task.Yield();
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            return 123;
        }

        public static async ValueTask<int> M4(T1 s1, T2 s2)
        {
            await Task.Yield();
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            return 123;
        }
    }

    [Async]
    public class Proxy_Class54<T1, T3>
    {
        public async Task<T3> M1<T2>(T1 s1, T2 s2)
        {
            await Task.Yield();
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            return default;
        }

        public async ValueTask<int> M2<T2>(T1 s1, T2 s2)
        {
            await Task.Yield();
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            return 123;
        }
        public static async Task<T4> M3<T2, T4>(T1 s1, T2 s2)
        {
            await Task.Yield();
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            return default;
        }

        public static async ValueTask<int> M4<T2>(T1 s1, T2 s2)
        {
            await Task.Yield();
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            return 123;
        }
    }
}
