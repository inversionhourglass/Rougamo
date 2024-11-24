using Rougamo;
using Rougamo.Context;
using Rougamo.Metadatas;
using System.Threading.Tasks;

namespace BasicUsage.Mos
{
    [Pointcut(AccessFlags.Constructor | AccessFlags.All)]
    public struct CtorValueMo : IMo
    {
        public double Order => 1;

        public Omit MethodContextOmits => Omit.None;

        public ForceSync ForceSync => ForceSync.None;

        public void OnEntry(MethodContext context)
        {
        }

        public ValueTask OnEntryAsync(MethodContext context)
        {
            OnEntry(context);
            return default;
        }

        public void OnException(MethodContext context)
        {
        }

        public ValueTask OnExceptionAsync(MethodContext context)
        {
            OnException(context);
            return default;
        }

        public void OnExit(MethodContext context)
        {
            this.SetOnEntry(context);
        }

        public ValueTask OnExitAsync(MethodContext context)
        {
            OnExit(context);
            return default;
        }

        public void OnSuccess(MethodContext context)
        {
        }

        public ValueTask OnSuccessAsync(MethodContext context)
        {
            OnSuccess(context);
            return default;
        }
    }
}
