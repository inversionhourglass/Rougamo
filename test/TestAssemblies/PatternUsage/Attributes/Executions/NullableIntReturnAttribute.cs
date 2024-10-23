using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Executions
{
    [Pointcut("execution(int? *(..))")]
    public class NullableIntReturnAttribute : SetOnEntryAttribute
    {
    }
}
