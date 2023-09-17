namespace PatternUsage.Attributes.Setters
{
    public class ReturnsChildOfIDictionaryAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "setter(System.Collections.IDictionary+ *)";
    }
}
