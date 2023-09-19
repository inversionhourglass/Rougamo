namespace PatternUsage.Attributes.Methods
{
    public class SpecificNestedTypeAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "method(* PatternUsage.X.NestedType/Inner.*(..))";
    }
}
