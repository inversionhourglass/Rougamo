using Rougamo.Context;
using System;

namespace Rougamo.ImplAssembly
{
    public class PublicMo : IMo
    {
        public AccessFlags Flags { get; } = AccessFlags.Public;

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
