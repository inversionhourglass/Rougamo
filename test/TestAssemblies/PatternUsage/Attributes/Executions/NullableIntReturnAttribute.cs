namespace PatternUsage.Attributes.Executions
{
    public class NullableIntReturnAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "execution(int? *(..))";
    }
}
