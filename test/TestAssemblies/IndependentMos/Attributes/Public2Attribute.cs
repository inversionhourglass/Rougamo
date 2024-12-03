using Rougamo;
using Rougamo.Metadatas;

namespace IndependentMos.Attributes
{
    [Pointcut(AccessFlags.Public)]
    [Advice(Feature.OnEntry)]
    public class Public2Attribute : SetOnEntryMoAttribute
    {
    }
}
