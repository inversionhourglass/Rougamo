using Rougamo.Context;
using System;

namespace Rougamo.ImplAssembly
{
    public class InstanceMoAttribute : MoAttribute
    {
        public InstanceMoAttribute()
        {
            Flags = AccessFlags.Instance;
        }

        public override AccessFlags Flags { get; }

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
