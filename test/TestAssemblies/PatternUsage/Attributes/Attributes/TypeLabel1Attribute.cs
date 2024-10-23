using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Attributes
{
    [Pointcut("attr(type PatternUsage.Attributes.Attributes.Label1Attribute)")]
    public class TypeLabel1Attribute : SetOnEntryAttribute
    {
    }
}
