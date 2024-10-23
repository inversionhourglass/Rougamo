using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Setters
{
    [Pointcut("setter(System.Collections.IDictionary+ *)")]
    public class ReturnsChildOfIDictionaryAttribute : SetOnEntryAttribute
    {
    }
}
