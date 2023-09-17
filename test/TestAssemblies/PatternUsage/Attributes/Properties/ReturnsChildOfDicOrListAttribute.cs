namespace PatternUsage.Attributes.Properties
{
    public class ReturnsChildOfDicOrListAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "property(IDictionary<string,string>+||IList<string>+ *)";
    }
}
