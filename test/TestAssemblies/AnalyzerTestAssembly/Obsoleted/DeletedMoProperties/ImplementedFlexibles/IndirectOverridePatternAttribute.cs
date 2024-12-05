using Rougamo;

namespace AnalyzerTestAssembly.Obsoleted.DeletedMoProperties.ImplementedFlexibles
{
    internal class IndirectOverridePatternAttribute : FullFlexiableAttribute
    {
        public override AccessFlags Flags { get; set; }

        public override string? Pattern { get; set; }

        public override double Order { get; set; }
    }
}
