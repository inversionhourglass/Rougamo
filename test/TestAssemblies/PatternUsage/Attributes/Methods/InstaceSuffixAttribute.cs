namespace PatternUsage.Attributes.Methods
{
    public class InstaceSuffixAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "method(* *.*Instance(..))";
    }
}
