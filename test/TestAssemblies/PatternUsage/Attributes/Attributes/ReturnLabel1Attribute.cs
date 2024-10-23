using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Attributes
{
    [Pointcut("attr(ret *.Attributes.*.Label1Attribute)")]
    public class ReturnLabel1Attribute : SetOnEntryAttribute
    {
    }
}
