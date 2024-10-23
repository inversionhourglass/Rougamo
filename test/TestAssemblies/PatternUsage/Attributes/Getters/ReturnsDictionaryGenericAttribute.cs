using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Getters
{
    [Pointcut("getter(Dictionary<..> *)")]
    public class ReturnsDictionaryGenericAttribute : SetOnEntryAttribute
    {
    }
}
