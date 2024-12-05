using Rougamo;
using Rougamo.Flexibility;

namespace AnalyzerTestAssembly.Obsoleted.DeletedMoProperties.ImplementedFlexibles
{
    internal class FullFlexiableAttribute : MoAttribute, IFlexiblePatternPointcut, IFlexibleModifierPointcut, IFlexibleOrderable
    {
        public virtual AccessFlags Flags { get; set; } = AccessFlags.All | AccessFlags.Method;

        public virtual string? Pattern { get; set; } = "method(* *(..))";

        public virtual double Order { get; set; } = 1;
    }
}
