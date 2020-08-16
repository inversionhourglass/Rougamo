using Rougamo.Context;
using System;

namespace Rougamo.ImplAssembly
{
    public class AllMo : IMo
    {
        public AccessFlags Flags => AccessFlags.All;

        public void OnEntry(EntryContext context)
        {
            throw new NotImplementedException();
        }

        public void OnException(ExceptionContext context)
        {
            throw new NotImplementedException();
        }

        public void OnExit(ExitContext context)
        {
            throw new NotImplementedException();
        }

        public void OnSuccess(SuccessContext context)
        {
            throw new NotImplementedException();
        }
    }
}
