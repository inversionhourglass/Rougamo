using Rougamo.Context;
using System;

namespace Rougamo.NugetImplAssembly
{
    public class AllMo : IMo
    {
        public AccessFlags Flags => AccessFlags.All;

        public void OnEntry(MethodContext context)
        {
            
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
