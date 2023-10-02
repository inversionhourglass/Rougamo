namespace PatternUsage.Attributes.Methods
{
    public class GenericParameterMatch1Attribute : SetOnEntryAttribute
    {
        public GenericParameterMatch1Attribute()
        {
            Pattern = "method(* *<TA,TB>.*<TC,TD>(TA,TB,TC,TD))";
        }

        public override string? Pattern { get; set; }
    }
}
