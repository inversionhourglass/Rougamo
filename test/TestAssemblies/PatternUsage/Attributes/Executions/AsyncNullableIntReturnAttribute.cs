namespace PatternUsage.Attributes.Executions
{
    public class AsyncNullableIntReturnAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "execution(async int? *(..))";
    }
}
