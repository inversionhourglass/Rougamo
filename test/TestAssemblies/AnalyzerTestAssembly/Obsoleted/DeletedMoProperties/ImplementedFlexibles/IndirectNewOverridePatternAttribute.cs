using Rougamo;

namespace AnalyzerTestAssembly.Obsoleted.DeletedMoProperties.ImplementedFlexibles
{
    internal class IndirectNewOverridePatternAttribute : IndirectOverridePatternAttribute
    {
        public new AccessFlags Flags => AccessFlags.All | AccessFlags.Constructor;

        public new string? Pattern => "ctor(*(..))";

        public new double Order => 2;
    }
}
