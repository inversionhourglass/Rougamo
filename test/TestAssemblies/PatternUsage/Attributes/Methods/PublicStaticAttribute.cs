using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Methods
{
    [Pointcut("method(public static * *(..))")]
    public class PublicStaticAttribute : SetOnEntryAttribute
    {
    }
}
