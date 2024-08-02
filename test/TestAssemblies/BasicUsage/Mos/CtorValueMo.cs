using Rougamo;
using Rougamo.Context;
using System.Threading.Tasks;

namespace BasicUsage.Mos
{
    public struct CtorValueMo : IMo
    {
        public AccessFlags Flags => AccessFlags.Constructor | AccessFlags.All;

        public string Pattern => null;

        public Feature Features => Feature.All;

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
