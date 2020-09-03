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

        public override void OnEntry(MethodContext context)
        {
            throw new NotImplementedException();
        }

        public override void OnException(MethodContext context)
        {
            throw new NotImplementedException();
        }

        public override void OnExit(MethodContext context)
        {
            throw new NotImplementedException();
        }

        public override void OnSuccess(MethodContext context)
        {
            throw new NotImplementedException();
        }
    }
}
