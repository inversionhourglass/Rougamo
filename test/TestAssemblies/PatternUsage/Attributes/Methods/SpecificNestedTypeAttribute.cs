using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Methods
{
    [Pointcut("method(* PatternUsage.X.NestedType/Inner.*(..))")]
    public class SpecificNestedTypeAttribute : SetOnEntryAttribute
    {
    }
}
