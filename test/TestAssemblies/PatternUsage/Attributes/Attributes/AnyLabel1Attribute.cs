namespace PatternUsage.Attributes.Attributes
{
    public class AnyLabel1Attribute : SetOnEntryAttribute
    {
        public override string? Pattern => "attr(* Label1Attribute)";
    }
}
