namespace PatternUsage.Attributes.Regexes
{
    public class NumberSuffixAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => @"regex(\w+ (static )?\S+ \S+?\d\()";
    }
}
