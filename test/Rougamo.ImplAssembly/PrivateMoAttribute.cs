using Rougamo.Context;
using System;

namespace Rougamo.ImplAssembly
{
    public class PrivateMoAttribute : MoAttribute
    {
        public override AccessFlags Flags => AccessFlags.NonPublic;

        public override void OnEntry(EntryContext context)
        {
            throw new NotImplementedException();
        }

        public override void OnException(ExceptionContext context)
        {
            throw new NotImplementedException();
        }

        public override void OnExit(ExitContext context)
        {
            throw new NotImplementedException();
        }

        public override void OnSuccess(SuccessContext context)
        {
            throw new NotImplementedException();
        }
    }
}
