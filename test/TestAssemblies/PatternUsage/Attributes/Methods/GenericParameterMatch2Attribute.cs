using Rougamo.Flexibility;

namespace PatternUsage.Attributes.Methods
{
    public class GenericParameterMatch2Attribute : SetOnEntryAttribute, IFlexiblePatternPointcut
    {
        public string? Pattern { get; set; } = "method(* *<TA,TB>.*<TC,TD>(TB,TA,TD,TC))";
    }
}
