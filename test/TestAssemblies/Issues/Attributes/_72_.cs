using Rougamo;
using Rougamo.Context;
using Rougamo.Metadatas;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Issues.Attributes
{
    [Advice(Feature.OnEntry)]
    [Pointcut(AccessFlags.All)]
    public struct _72_ : IMo
    {
        public _72_() { }

        public double Order { get; } = 1;

        public Omit MethodContextOmits { get; } = Omit.None;

        public ForceSync ForceSync => ForceSync.None;

        public void OnEntry(MethodContext context)
        {
            var list = (List<string>)context.Arguments[0];
            list.Add(nameof(OnEntry));
        }

        public ValueTask OnEntryAsync(MethodContext context)
        {
            OnEntry(context);
            return default;
        }

        public void OnException(MethodContext context)
        {
            var list = (List<string>)context.Arguments[0];
            list.Add(nameof(OnException));
        }

        public ValueTask OnExceptionAsync(MethodContext context)
        {
            OnException(context);
            return default;
        }

        public void OnExit(MethodContext context)
        {
            var list = (List<string>)context.Arguments[0];
            list.Add(nameof(OnExit));
        }

        public ValueTask OnExitAsync(MethodContext context)
        {
            OnExit(context);
            return default;
        }

        public void OnSuccess(MethodContext context)
        {
            var list = (List<string>)context.Arguments[0];
            list.Add(nameof(OnSuccess));
        }

        public ValueTask OnSuccessAsync(MethodContext context)
        {
            OnSuccess(context);
            return default;
        }
    }
}
