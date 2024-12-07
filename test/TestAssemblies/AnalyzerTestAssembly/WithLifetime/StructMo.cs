using Rougamo;
using Rougamo.Context;
using Rougamo.Metadatas;

namespace AnalyzerTestAssembly.WithLifetime
{
    [Lifetime(Lifetime.Singleton)]
    internal struct StructMo : ISyncMo
    {
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
