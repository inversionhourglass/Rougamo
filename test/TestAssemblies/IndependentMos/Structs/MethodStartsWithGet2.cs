using Rougamo;
using Rougamo.Context;
using Rougamo.Metadatas;
using System.Threading.Tasks;

namespace IndependentMos.Structs
{
    [Pointcut("method(* *.Get*(..))")]
    [Advice(Feature.OnEntry)]
    public struct MethodStartsWithGet2 : IMo
    {
        public void OnEntry(MethodContext context)
        {
            this.SetOnEntry(context);
        }

        public ValueTask OnEntryAsync(MethodContext context)
        {
            OnEntry(context);
            return new ValueTask();
        }

        public void OnException(MethodContext context)
        {
            
        }

        public ValueTask OnExceptionAsync(MethodContext context)
        {
            return new ValueTask();
        }

        public void OnExit(MethodContext context)
        {
            
        }

        public ValueTask OnExitAsync(MethodContext context)
        {
            return new ValueTask();
        }

        public void OnSuccess(MethodContext context)
        {
            
        }

        public ValueTask OnSuccessAsync(MethodContext context)
        {
            return new ValueTask();
        }
    }
}
