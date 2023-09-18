namespace PatternUsage.Attributes.Methods
{
    public class AnyMethodGenericAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "method(* *<..>(..))";
    }
}
