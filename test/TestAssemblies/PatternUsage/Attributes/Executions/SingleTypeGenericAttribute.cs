namespace PatternUsage.Attributes.Executions
{
    public class SingleTypeGenericAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "execution(* *<>.*(..))";
    }
}
