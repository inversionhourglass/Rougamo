using System;
using System.Collections.Generic;
using System.Text;

namespace Rougamo.UsingAssembly
{
    public class Test
    {
        public int TryCatchFinallyReturn(string str, int v1)
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

        public void TryCatchFinallyVoid(string str, int v1)
        {
            try
            {
                Console.WriteLine(str);
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

        public void TryCatchVoid(string str, int v1)
        {
            try
            {
                Console.WriteLine(str);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        public int TryCatchReturn(string str, int v1)
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
        }

        public int TryFinallyReturn(string str, int v1)
        {
            try
            {
                Console.WriteLine(str);
                return 123;
            }
            finally
            {
                Console.WriteLine(v1);
            }
        }

        public void TryFinallyVoid(string str, int v1)
        {
            try
            {
                Console.WriteLine(str);
            }
            finally
            {
                Console.WriteLine(v1);
            }
        }
    }
}
