namespace PatternUsage.Attributes.Attributes
{
    public class AnyParameterLabel1Attribute : SetOnEntryAttribute
    {
        public override string? Pattern => "attr(para * PatternUsage.*.*.Label1Attribute)";
    }
}
