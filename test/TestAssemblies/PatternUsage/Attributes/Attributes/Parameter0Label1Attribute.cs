using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Attributes
{
    [Pointcut("attr(para 0 *..Label1Attribute)")]
    public class Parameter0Label1Attribute : SetOnEntryAttribute
    {
    }
}
