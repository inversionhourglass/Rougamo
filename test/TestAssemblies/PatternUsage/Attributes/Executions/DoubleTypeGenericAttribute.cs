namespace PatternUsage.Attributes.Executions
{
    public class DoubleTypeGenericAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "execution(* *<,>.*(..))";
    }
}
