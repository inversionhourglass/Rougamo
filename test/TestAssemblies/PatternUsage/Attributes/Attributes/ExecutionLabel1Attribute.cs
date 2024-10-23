using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Attributes
{
    [Pointcut("attr(exec PatternUsage..Label1Attribute)")]
    public class ExecutionLabel1Attribute : SetOnEntryAttribute
    {
    }
}
