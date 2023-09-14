namespace PatternUsage.Attributes.Executions
{
    public class ChildOfInterfaceAWidlyAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "execution(* InterfaceA.*(..))";
    }
}
