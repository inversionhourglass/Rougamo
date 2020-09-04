using Rougamo.Context;
using System;

namespace Rougamo.NugetImplAssembly
{
    public class ObsoleteProxyMoAttribute : MoAttribute
    {
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
