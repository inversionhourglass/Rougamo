namespace PatternUsage.Attributes.Methods
{
    public class PublicStaticAttribute : SetOnEntryAttribute
    {
        public override string Pattern => "method(public static * *(..))";
    }
}
