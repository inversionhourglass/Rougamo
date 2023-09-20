namespace PatternUsage.Attributes.Methods
{
    public class DoubleAnyParameterAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "method(* *(,))";
    }
}
