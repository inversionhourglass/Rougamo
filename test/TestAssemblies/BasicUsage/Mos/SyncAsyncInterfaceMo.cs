#if NETCOREAPP3_1
using Rougamo;
using Rougamo.Context;
using Rougamo.Metadatas;
using System.Threading.Tasks;

namespace BasicUsage.Mos
{
    [Advice(Feature.OnEntry)]
    [Lifetime(Lifetime.Singleton)]
    public class ImplementISyncMoClass : ISyncMo
    {
        public void OnEntry(MethodContext context)
        {
            this.SetOnEntry(context);
        }

        public void OnException(MethodContext context)
        {
            
        }

        public void OnExit(MethodContext context)
        {
            
        }

        public void OnSuccess(MethodContext context)
        {
            
        }
    }

    [Advice(Feature.OnEntry)]
    [Lifetime(Lifetime.Singleton)]
    public class ImplementIAsyncMoClass : IAsyncMo
    {
        public ValueTask OnEntryAsync(MethodContext context)
        {
            this.SetOnEntry(context);

            return default;
        }

        public ValueTask OnExceptionAsync(MethodContext context)
        {
            return default;
        }

        public ValueTask OnExitAsync(MethodContext context)
        {
            return default;
        }

        public ValueTask OnSuccessAsync(MethodContext context)
        {
            return default;
        }
    }

    [Advice(Feature.OnEntry)]
    [Lifetime(Lifetime.Singleton)]
    public struct ImplementISyncMoStruct : ISyncMo
    {
        public void OnEntry(MethodContext context)
        {
            this.SetOnEntry(context);
        }

        public void OnException(MethodContext context)
        {

        }

        public void OnExit(MethodContext context)
        {

        }

        public void OnSuccess(MethodContext context)
        {

        }
    }

    [Advice(Feature.OnEntry)]
    [Lifetime(Lifetime.Singleton)]
    public struct ImplementIAsyncMoStruct : IAsyncMo
    {
        public ValueTask OnEntryAsync(MethodContext context)
        {
            this.SetOnEntry(context);

            return default;
        }

        public ValueTask OnExceptionAsync(MethodContext context)
        {
            return default;
        }

        public ValueTask OnExitAsync(MethodContext context)
        {
            return default;
        }

        public ValueTask OnSuccessAsync(MethodContext context)
        {
            return default;
        }
    }
}
#endif
