using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Regexes
{
    [Pointcut(@"regex(\w+ (static )?\S+ \S+?\d\()")]
    public class NumberSuffixAttribute : SetOnEntryAttribute
    {
    }
}
