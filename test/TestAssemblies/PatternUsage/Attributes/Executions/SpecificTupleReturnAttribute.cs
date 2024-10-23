using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Executions
{
    [Pointcut("execution((int,string) *(..))")]
    public class SpecificTupleReturnAttribute : SetOnEntryAttribute
    {
    }
}
