using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Attributes
{
    [Pointcut("attr(* Label1Attribute)")]
    public class AnyLabel1Attribute : SetOnEntryAttribute
    {
    }
}
