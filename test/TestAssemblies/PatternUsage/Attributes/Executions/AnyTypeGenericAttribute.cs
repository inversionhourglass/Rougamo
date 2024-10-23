using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Executions
{
    [Pointcut("execution(* *<..>.*(..))")]
    public class AnyTypeGenericAttribute : SetOnEntryAttribute
    {
    }
}
