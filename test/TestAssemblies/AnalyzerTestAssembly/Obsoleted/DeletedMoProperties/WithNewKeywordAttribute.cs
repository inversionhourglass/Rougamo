using Rougamo;

namespace AnalyzerTestAssembly.Obsoleted.DeletedMoProperties
{
    internal class WithNewKeywordAttribute : MoAttribute
    {
#if ALLOWED_COMPILER_ERROR
        public new AccessFlags Flags { get; set; }
        
        public new string? Pattern { get; set; }

        public new Feature Features { get; }

        public new Rougamo.Context.Omit MethodContextOmits { get; }

        public new ForceSync ForceSync { get; }
        
        public new double Order { get; set; } = 1;
#endif
    }
}
