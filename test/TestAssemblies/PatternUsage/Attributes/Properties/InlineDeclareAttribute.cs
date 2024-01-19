namespace PatternUsage.Attributes.Properties
{
    public class InlineDeclareAttribute : SetOnEntryAttribute
    {
        public new string? Pattern { get; set; } = "property(* *)";
    }
}
