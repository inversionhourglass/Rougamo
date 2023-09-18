namespace PatternUsage.Attributes.Executions
{
    public class SingleMethodGenericAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "execution(* *<>(..))";
    }
}
