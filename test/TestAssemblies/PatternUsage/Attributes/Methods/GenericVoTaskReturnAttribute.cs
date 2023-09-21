namespace PatternUsage.Attributes.Methods
{
    public class GenericVoTaskReturnAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "method(async * *(..))";
    }
}
