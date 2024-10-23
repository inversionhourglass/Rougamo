using Rougamo.Metadatas;

namespace PatternUsage.Attributes
{
    [Pointcut(@"regex(\w+ (static )?\S+ \S+?\d\() && method(* *(,))")]
    public class IntegrativeAttribute : SetOnEntryAttribute
    {
    }
}
