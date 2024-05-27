using Rougamo;
using Rougamo.Context;
using System.Threading.Tasks;

namespace IndirectDependency1.Mos
{
    public struct Id1ValueMo : IMo
    {
        public AccessFlags Flags => AccessFlags.InstancePublic;

        public string Pattern => null;

        public Feature Features => Feature.All;

        public double Order => 0;

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

    public struct Id1ValueMo<T> : IMo
    {
        public AccessFlags Flags => AccessFlags.InstancePublic;

        public string Pattern => null;

        public Feature Features => Feature.All;

        public double Order => 0;

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
