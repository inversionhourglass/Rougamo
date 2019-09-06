using Rougamo.Context;
using System;

namespace Rougamo.TestAssembly
{
    public class TheMo : IMo
    {
        public void OnEntry(EntryContext context)
        {
            Console.WriteLine(nameof(OnEntry));
        }

        public void OnException(ExceptionContext context)
        {
            Console.WriteLine(nameof(OnException));
        }

        public void OnExit(ExitContext context)
        {
            Console.WriteLine(nameof(OnExit));
        }

        public void OnSuccess(SuccessContext context)
        {
            Console.WriteLine(nameof(OnSuccess));
        }
    }

    public interface IBaseMo : IRougamo<TheMo> { }
}
