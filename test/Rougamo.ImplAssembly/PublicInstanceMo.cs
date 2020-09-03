using Rougamo.Context;
using System;

namespace Rougamo.ImplAssembly
{
    public class PublicInstanceMo : IMo
    {
        public AccessFlags Flags => AccessFlags.Public & AccessFlags.Instance;

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
