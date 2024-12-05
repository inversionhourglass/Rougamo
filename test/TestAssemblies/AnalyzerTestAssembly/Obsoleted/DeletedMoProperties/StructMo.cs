using Rougamo;
using Rougamo.Context;

namespace AnalyzerTestAssembly.Obsoleted.DeletedMoProperties
{
    internal class StructMo : IMo
    {
        public AccessFlags Flags => AccessFlags.All;

        public string? Pattern => "method(* *(..))";

        public Feature Features => Feature.All;

        public double Order => double.MaxValue;

        public Omit MethodContextOmits => Omit.All;

        public ForceSync ForceSync => ForceSync.All;

        public void OnEntry(MethodContext context)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnEntryAsync(MethodContext context)
        {
            throw new NotImplementedException();
        }

        public void OnException(MethodContext context)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnExceptionAsync(MethodContext context)
        {
            throw new NotImplementedException();
        }

        public void OnExit(MethodContext context)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnExitAsync(MethodContext context)
        {
            throw new NotImplementedException();
        }

        public void OnSuccess(MethodContext context)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnSuccessAsync(MethodContext context)
        {
            throw new NotImplementedException();
        }
    }
}
