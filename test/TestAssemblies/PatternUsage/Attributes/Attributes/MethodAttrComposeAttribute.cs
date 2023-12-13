namespace PatternUsage.Attributes.Attributes
{
    public class MethodAttrComposeAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "method(* *.AttributeCompose*(..)) && !attr(para 0 Label1Attribute)";
    }
}
