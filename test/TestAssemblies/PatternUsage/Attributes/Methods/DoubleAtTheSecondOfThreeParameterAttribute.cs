using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Methods
{
    [Pointcut("method(* *(,double,))")]
    public class DoubleAtTheSecondOfThreeParameterAttribute : SetOnEntryAttribute
    {
    }
}
