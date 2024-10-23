using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Methods
{
    [Pointcut("method(* *.*Instance(..))")]
    public class InstaceSuffixAttribute : SetOnEntryAttribute
    {
    }
}
