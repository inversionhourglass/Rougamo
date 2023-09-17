namespace PatternUsage.Attributes.Getters
{
    public class ReturnsDictionaryGenericAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => "getter(Dictionary<..> *)";
    }
}
