using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Methods
{
    [Pointcut("method(* */*.*(..))")]
    public class AnyDoubleDepthNestedTypeAttribute : SetOnEntryAttribute
    {
    }
}
