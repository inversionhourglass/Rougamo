using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Methods
{
    [Pointcut("method(async double||int *(..))")]
    public class DoubleOrIntVoTaskReturnAttribute : SetOnEntryAttribute
    {
    }
}
