using Rougamo.Context;
using Rougamo;
using System.Threading.Tasks;
using Rougamo.Metadatas;

namespace BasicUsage.Mos
{
    [Advice(Feature.EntryReplace)]
    [Pointcut(AccessFlags.All)]
    public struct ValueOmitAll : IMo
    {
        public double Order => 1;

        public Omit MethodContextOmits => Omit.All;

        public ForceSync ForceSync => ForceSync.None;

        public void OnEntry(MethodContext context)
        {
            if (context.Arguments.Length == 0 && context.Mos.Count == 0)
            {
                this.SetOnEntry(context);
            }
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
