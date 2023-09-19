namespace PatternUsage.Attributes.Methods
{
    public class AnyDoubleDepthNestedTypeAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "method(* */*.*(..))";
    }
}
