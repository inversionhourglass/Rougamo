using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Executions
{
    [Pointcut("execution(* InterfaceA+.*(..))")]
    public class ChildOfInterfaceAWidlyAttribute : SetOnEntryAttribute
    {
    }
}
