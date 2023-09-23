namespace PatternUsage.Attributes.Properties
{
    public class InlineDeclareAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "property(* *)";
    }
}
