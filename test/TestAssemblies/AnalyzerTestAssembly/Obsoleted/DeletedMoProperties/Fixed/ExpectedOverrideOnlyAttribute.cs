using Rougamo;
using Rougamo.Flexibility;
using Rougamo.Metadatas;

namespace AnalyzerTestAssembly.Obsoleted.DeletedMoProperties.Fixed
{
    [Pointcut(AccessFlags.Instance)]
    [Advice(Feature.All)]
    [Optimization(ForceSync = ForceSync.None, MethodContext = Rougamo.Context.Omit.None)]
    internal class ExpectedOverrideOnlyAttribute : AsyncMoAttribute, IFlexibleOrderable
    {
        public double Order { get; set; } = 2;
    }
}
