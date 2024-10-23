using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Properties
{
    [Pointcut("property(IDictionary<string,string>+||IList<string>+ *)")]
    public class ReturnsChildOfDicOrListAttribute : SetOnEntryAttribute
    {
    }
}
