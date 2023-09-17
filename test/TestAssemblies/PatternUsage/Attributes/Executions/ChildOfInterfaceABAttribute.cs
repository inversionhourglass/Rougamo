namespace PatternUsage.Attributes.Executions
{
    public class ChildOfInterfaceABAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "execution(* PatternUsage.InterfaceAB+.*(..))";
    }
}
