namespace PatternUsage.Attributes.Methods
{
    public class GenericParameterMatch2Attribute : SetOnEntryAttribute
    {
        public override string? Pattern { get; set; } = "method(* *<TA,TB>.*<TC,TD>(TB,TA,TD,TC))";
    }
}
