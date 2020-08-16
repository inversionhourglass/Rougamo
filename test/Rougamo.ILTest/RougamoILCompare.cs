using System;
using System.IO;
using System.Reflection;

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
            var _ = new TheMo();
            var @out = new Rougamo.Context.ExitContext(this, typeof(RougamoILCompare).GetMethod("SyncWrap", BindingFlags.Public | BindingFlags.Instance), k, p);
            var @in = new Rougamo.Context.EntryContext(@out);
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
                var @exception = new Rougamo.Context.ExceptionContext(@out, e);
                _.OnException(exception);
                throw e;
            }
            finally
            {
                _.OnExit(@out);
            }
        }
    }
}
