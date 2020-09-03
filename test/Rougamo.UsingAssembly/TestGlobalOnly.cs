using System;
using System.Collections.Generic;
using System.Text;

namespace Rougamo.UsingAssembly
{
    public class TestGlobalOnly
    {
        public static void Static(object[] objs, Tuple<int, int> tuple)
        {

        }

        public int Instance(string str, int v1)
        {
            try
            {
                Console.WriteLine(str);
                return 123;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
            finally
            {
                Console.WriteLine(v1);
            }
        }

        //private static void PrivateStatic(double db, Guid guid)
        //{

        //}

        //private void PrivateInstance(string[] arr)
        //{

        //}
    }
}
