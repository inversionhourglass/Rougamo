using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Attributes
{
    [Pointcut("attr(para * PatternUsage.*.*.Label1Attribute)")]
    public class AnyParameterLabel1Attribute : SetOnEntryAttribute
    {
    }
}
