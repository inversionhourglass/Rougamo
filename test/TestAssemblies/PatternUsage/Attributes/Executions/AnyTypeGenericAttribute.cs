namespace PatternUsage.Attributes.Executions
{
    public class AnyTypeGenericAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "execution(* *<..>.*(..))";
    }
}
