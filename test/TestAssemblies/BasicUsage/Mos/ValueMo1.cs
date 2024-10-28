using Rougamo;
using Rougamo.Context;
using Rougamo.Metadatas;
using System.Threading.Tasks;

namespace BasicUsage.Mos
{
    [Pointcut("execution(async null *(..))")]
    public struct ValueMo1 : IMo
    {
        public Feature Features => Feature.OnEntry;

        public double Order => 1;

        public Omit MethodContextOmits => Omit.None;

        public ForceSync ForceSync => ForceSync.None;

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

        public ValueTask OnEntryAsync(MethodContext context)
        {
            OnEntry(context);
            return default;
        }

        public ValueTask OnExceptionAsync(MethodContext context)
        {
            OnException(context);
            return default;
        }

        public ValueTask OnSuccessAsync(MethodContext context)
        {
            OnSuccess(context);
            return default;
        }

        public ValueTask OnExitAsync(MethodContext context)
        {
            OnExit(context);
            return default;
        }
    }
}
