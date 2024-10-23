using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Methods
{
    [Pointcut("method(async null *(..))")]
    public class NoGenericVoTaskReturnAttribute : SetOnEntryAttribute
    {
    }
}
