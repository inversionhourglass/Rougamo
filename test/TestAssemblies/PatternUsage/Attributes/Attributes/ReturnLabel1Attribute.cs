namespace PatternUsage.Attributes.Attributes
{
    public class ReturnLabel1Attribute : SetOnEntryAttribute
    {
        public override string? Pattern => "attr(ret *.Attributes.*.Label1Attribute)";
    }
}
