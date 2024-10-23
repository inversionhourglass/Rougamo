using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Methods
{
    [Pointcut("method(!static * *(..))")]
    public class InstanceAttribute : SetOnEntryAttribute
    {
    }
}
