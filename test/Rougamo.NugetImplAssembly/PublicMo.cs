using Rougamo.Context;
using System;

namespace Rougamo.NugetImplAssembly
{
    public class PublicMo : IMo
    {
        public AccessFlags Flags { get; } = AccessFlags.Public;

        public void OnEntry(MethodContext context)
        {
            throw new NotImplementedException();
        }

        public void OnException(MethodContext context)
        {
            throw new NotImplementedException();
        }

        public void OnExit(MethodContext context)
        {
            throw new NotImplementedException();
        }

        public void OnSuccess(MethodContext context)
        {
            throw new NotImplementedException();
        }
    }
}
