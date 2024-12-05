using Rougamo;
using Rougamo.Context;
using Rougamo.Flexibility;

namespace AnalyzerTestAssembly.Obsoleted.DeletedMoProperties.ImplementedFlexibles
{
    internal class FullFlexibleStructMo : IMo, IFlexibleModifierPointcut, IFlexiblePatternPointcut, IFlexibleOrderable
    {
        public double Order { get; set; }

        public string? Pattern { get; set; }

        public AccessFlags Flags { get; set; }

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
