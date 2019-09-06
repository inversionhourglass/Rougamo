using System;
using System.Reflection;

namespace FodyTestLib
{
    public class RougamoILCompare : IBaseMo
    {
        private Random _random;

        public void None()
        {
            try
            {
                Console.WriteLine("try");
                if (_random.Next(1, 100) > 30) return;
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                Console.WriteLine("finally");
            }
            Console.WriteLine("exit");
        }

        public void Wrap()
        {
            var _ = new TheMo();
            var exit = new Rougamo.Context.ExitContext(this, typeof(RougamoILCompare).GetMethod("SyncWrap", BindingFlags.Public | BindingFlags.Instance));
            var entry = new Rougamo.Context.EntryContext(exit);
            try
            {
                _.OnEntry(entry);
            }
            finally
            {
                _.OnExit(exit);
            }
            try
            {
                try
                {
                    Console.WriteLine("try");
                    if (_random.Next(1, 100) > 30) return;
                }
                catch (Exception)
                {
                    return;
                }
                finally
                {
                    Console.WriteLine("finally");
                }
                Console.WriteLine("exit");

                _.OnSuccess(new Rougamo.Context.SuccessContext(exit));
            }
            catch(Exception e)
            {
                _.OnException(new Rougamo.Context.ExceptionContext(exit) { Exception = e });
            }
            finally
            {
                _.OnExit(exit);
            }
        }
    }
}
