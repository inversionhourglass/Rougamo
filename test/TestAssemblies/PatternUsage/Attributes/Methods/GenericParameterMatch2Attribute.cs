namespace PatternUsage.Attributes.Methods
{
    public class GenericParameterMatch2Attribute : SetOnEntryAttribute
    {
        public new string? Pattern { get; } = "method(* *<TA,TB>.*<TC,TD>(TB,TA,TD,TC))";
    }
}
