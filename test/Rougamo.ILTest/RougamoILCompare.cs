using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Rougamo.ILTest
{
    public class RougamoILCompare
    {
        private Random _random;

        public string SingleResult(int a, float b, double c)
        {
            var objs = new object[3];
            objs[0] = 1;
            objs[1] = "2";
            objs[2] = 3.1561;

            var max = Math.Max(b, c);
            return a + max + "";
        }

        public int SyncNone(int k, int p)
        {
            _random = new Random(k|p);
            var i = 0;
            try
            {
                for (int j = 0; j < 15; j++)
                {
                    i += _random.Next(17, 347);
                }
                if (i < 345) throw new Exception("abc");
                if (i > 123456) return 0;
            }
            catch (IOException)
            {
                return -1;
            }
            return i;
        }

        public int SyncWrap(int k, int p)
        {
            var method = typeof(RougamoILCompare).GetMethod(nameof(SyncWrap), new[] { typeof(int), typeof(int) });
            var objs = new object[] { method, 1, 2.34, Guid.NewGuid() };
            var _ = new TheMoAttribute(AccessFlags.All, 123, new[] { 1.21, 2.34, 5.121 })
            {
                StringValue = "234",
                ObjectValue = null,
                IntArray = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 100 }
            };
            var context = new Rougamo.Context.MethodContext(this, typeof(RougamoILCompare), typeof(RougamoILCompare).GetMethod("SyncWrap", BindingFlags.Public | BindingFlags.Instance), new object[] { k, p });
            _.OnEntry(context);
            try
            {
                _random = new Random(k|p);
                var i = 0;
                try
                {
                    for (int j = 0; j < 15; j++)
                    {
                        i += _random.Next(17, 347);
                    }
                    if (i > 123456) return 0;
                }
                catch (IOException)
                {
                    return -1;
                }
                return i;
            }
            catch (Exception e)
            {
                context.Exception = e;
                _.OnException(context);
                throw e;
            }
            finally
            {
                _.OnExit(context);
            }
        }

        public async Task<string>AsyncNone(string a, Guid b)
        {
            Console.WriteLine(1);
            await Task.Delay(100);
            Console.WriteLine("2");
            return "3";
        }
    }
}
