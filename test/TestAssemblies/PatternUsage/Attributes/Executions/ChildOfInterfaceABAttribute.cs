using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Executions
{
    [Pointcut("execution(* PatternUsage.InterfaceAB+.*(..))")]
    public class ChildOfInterfaceABAttribute : SetOnEntryAttribute
    {
    }
}
