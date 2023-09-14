namespace PatternUsage.Attributes.Executions
{
    internal class ChildOfInterfaceABAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "execution(* PatternUsage.InterfaceAB.*.*(..))";
    }
}
