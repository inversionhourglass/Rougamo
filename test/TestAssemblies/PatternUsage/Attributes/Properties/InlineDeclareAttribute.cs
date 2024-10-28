using Rougamo.Flexibility;

namespace PatternUsage.Attributes.Properties
{
    public class InlineDeclareAttribute : SetOnEntryAttribute, IFlexiblePatternPointcut
    {
        public string? Pattern { get; set; } = "property(* *)";
    }
}
