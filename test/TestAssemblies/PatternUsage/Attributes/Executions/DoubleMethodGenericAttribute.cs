namespace PatternUsage.Attributes.Executions
{
    public class DoubleMethodGenericAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "execution(* *<,>(..))";
    }
}
