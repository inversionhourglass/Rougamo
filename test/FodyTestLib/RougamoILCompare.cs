using System;
using System.IO;
using System.Reflection;

namespace FodyTestLib
{
    public class RougamoILCompare : IBaseMo
    {
        private Random _random;

        public string SingleResult(int a, float b, double c)
        {
            var max = Math.Max(b, c);
            return a + max + "";
        }

        public int SyncNone(int k)
        {
            _random = new Random(k);
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

        public int SyncWrap(int k)
        {
            var _ = new TheMo();
            var @out = new Rougamo.Context.OutContext(this, typeof(RougamoILCompare).GetMethod("SyncWrap", BindingFlags.Public | BindingFlags.Instance), k);
            var @in = new Rougamo.Context.InContext(@out);
            _.Before(@in);
            if (@in.Interrupt)
            {
                if (@in.Throw != null) throw @in.Throw;
                return (int)@in.ReplacedReturn;
            }
            try
            {
                _random = new Random(k);
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
            catch(Exception e)
            {
                return 1;
            }
            finally
            {
            }
        }
    }
}
