namespace PatternUsage.Attributes.Executions
{
    public class AnyTwoItemTupleParameterAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "execution(* *((*,*)))";
    }
}
