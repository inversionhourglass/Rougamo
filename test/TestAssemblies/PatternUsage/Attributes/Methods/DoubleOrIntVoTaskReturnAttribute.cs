namespace PatternUsage.Attributes.Methods
{
    public class DoubleOrIntVoTaskReturnAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "method(async double||int *(..))";
    }
}
