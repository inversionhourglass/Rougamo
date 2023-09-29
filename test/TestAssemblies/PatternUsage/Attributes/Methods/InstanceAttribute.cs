namespace PatternUsage.Attributes.Methods
{
    public class InstanceAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "method(!static * *(..))";
    }
}
