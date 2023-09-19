namespace PatternUsage.Attributes.Executions
{
    public class AnyTypeGenericNoneMethodGenericAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "execution(* *<..>.*<!>(..))";
    }
}
