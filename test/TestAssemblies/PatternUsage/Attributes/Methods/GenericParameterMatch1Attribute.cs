using Rougamo.Metadatas;

namespace PatternUsage.Attributes.Methods
{
    [Pointcut("method(* *<TA,TB>.*<TC,TD>(TA,TB,TC,TD))")]
    public class GenericParameterMatch1Attribute : SetOnEntryAttribute
    {
    }
}
