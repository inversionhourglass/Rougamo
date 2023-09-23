namespace PatternUsage.Attributes
{
    public class IntegrativeAttribute : SetOnEntryAttribute
    {
        public override string? Pattern => @"regex(\w+ (static )?\S+ \S+?\d\() && method(* *(,))";
    }
}
