namespace PatternUsage.Attributes.Executions
{
    public class SpecificTupleReturnAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "execution((int,string) *(..))";
    }
}
