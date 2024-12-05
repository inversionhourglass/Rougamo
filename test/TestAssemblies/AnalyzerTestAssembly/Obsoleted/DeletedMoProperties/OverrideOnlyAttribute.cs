using Rougamo;

namespace AnalyzerTestAssembly.Obsoleted.DeletedMoProperties
{
    internal class OverrideOnlyAttribute : AsyncMoAttribute
    {
#if ALLOWED_COMPILER_ERROR
        public override AccessFlags Flags => AccessFlags.Instance;
        
        public override string? Pattern => null;

        public override Feature Features => Feature.All;

        public override Rougamo.Context.Omit MethodContextOmits => Rougamo.Context.Omit.None;

        public override ForceSync ForceSync => ForceSync.None;
        
        public override double Order => 2;
#endif
    }
}
