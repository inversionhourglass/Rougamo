namespace PatternUsage.Attributes.Attributes
{
    public class ExecutionLabel1Attribute : SetOnEntryAttribute
    {
        public override string? Pattern => "attr(exec PatternUsage..Label1Attribute)";
    }
}
