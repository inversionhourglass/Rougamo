using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Methods
{
    [Pointcut("method(* *<..>(..))")]
    public class AnyMethodGenericAttribute : SetOnEntryAttribute
    {
    }
}
