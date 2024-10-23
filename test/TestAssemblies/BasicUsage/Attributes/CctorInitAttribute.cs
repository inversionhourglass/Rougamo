using Rougamo;
using Rougamo.Context;
using Rougamo.Metadatas;

namespace BasicUsage.Attributes
{
    [Pointcut(AccessFlags.Static | AccessFlags.Constructor)]
    public class CctorInitAttribute : MoAttribute
    {
        public const string FLAG = nameof(CctorInitAttribute);

        public override void OnEntry(MethodContext context)
        {
            SyncExecution.StaticProp1 = FLAG;
        }
    }
}
