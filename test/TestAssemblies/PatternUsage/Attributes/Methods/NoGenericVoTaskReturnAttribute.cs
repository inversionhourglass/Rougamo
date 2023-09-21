namespace PatternUsage.Attributes.Methods
{
    public class NoGenericVoTaskReturnAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "method(async null *(..))";
    }
}
