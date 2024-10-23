using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Methods
{
    [Pointcut("method(int[,][][,,] *(..))")]
    public class IntArrayReturnAttribute : SetOnEntryAttribute
    {
    }
}
