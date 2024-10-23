using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Executions
{
    [Pointcut("execution(async int? *(..))")]
    public class AsyncNullableIntReturnAttribute : SetOnEntryAttribute
    {
    }
}
