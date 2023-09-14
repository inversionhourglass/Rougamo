namespace PatternUsage.Attributes.Methods
{
    public class StaticAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "method(static * *(..))";
    }
}
