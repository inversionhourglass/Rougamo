using BasicUsage.Attributes;
using System;
using System.Data;
using System.Threading.Tasks;

namespace BasicUsage
{
    public class ModifyArguments
    {
        [RewriteArgs]
        public object[] Sync<T>(
            string a, sbyte b, int? c, object d, Type e, DbType f, DbType[] g, DateTime h, T i,
            ref sbyte j, ref int? k, ref DbType l, ref DbType[] m, ref T n,
            out string o, out object p, out Type q, out DateTime r, out T s)
        {
            o = default;
            p = default;
            q = default;
            r = default;
            s = default;
            return new object[] { a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s };
        }

        [RewriteArgs]
        public async Task<object[]> Async<T>(string a, sbyte b, int? c, object d, Type e, DbType f, DbType[] g, DateTime h, T i)
        {
            await Task.Yield();
            return new object[] { a, b, c, d, e, f, g, h, i };
        }
    }
}
