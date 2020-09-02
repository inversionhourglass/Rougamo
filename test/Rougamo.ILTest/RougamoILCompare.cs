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
            var objs = new object[] { null, 1, 2.34, Guid.NewGuid() };
            var _ = new TheMoAttribute(AccessFlags.All, 123, new[] { 1.21, 2.34, 5.121 })
            {
                StringValue = "234",
                ObjectValue = null,
                IntArray = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 100 }
            };
            var @in = new Rougamo.Context.EntryContext(this, typeof(RougamoILCompare), typeof(RougamoILCompare).GetMethod("SyncWrap", BindingFlags.Public | BindingFlags.Instance), k, p);
            _.OnEntry(@in);
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
                var @exception = new Rougamo.Context.ExceptionContext(e, this, typeof(RougamoILCompare), typeof(RougamoILCompare).GetMethod("SyncWrap", BindingFlags.Public | BindingFlags.Instance), k, p);
                _.OnException(exception);
                throw e;
            }
            finally
            {
                var @out = new Rougamo.Context.ExitContext(this, typeof(RougamoILCompare), typeof(RougamoILCompare).GetMethod("SyncWrap", BindingFlags.Public | BindingFlags.Instance), k, p);
                _.OnExit(@out);
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
