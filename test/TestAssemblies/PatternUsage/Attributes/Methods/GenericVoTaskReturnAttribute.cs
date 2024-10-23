using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Methods
{
    [Pointcut("method(async * *(..))")]
    public class GenericVoTaskReturnAttribute : SetOnEntryAttribute
    {
    }
}
