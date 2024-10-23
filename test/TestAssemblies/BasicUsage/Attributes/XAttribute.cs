using Rougamo;
using Rougamo.Metadatas;

namespace BasicUsage.Attributes
{
    [Pointcut("x")]
    internal class XAttribute : MoAttribute
    {
    }

    [Pointcut("y")]
    internal class YAttribute : MoAttribute
    {
    }

    [Pointcut("z")]
    internal class ZAttribute : MoAttribute
    {
    }
}
