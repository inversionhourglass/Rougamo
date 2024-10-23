using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Attributes
{
    [Pointcut("method(* *.AttributeCompose*(..)) && !attr(para 0 Label1Attribute)")]
    public class MethodAttrComposeAttribute : SetOnEntryAttribute
    {
    }
}
