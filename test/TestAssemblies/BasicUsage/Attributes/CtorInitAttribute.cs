using Rougamo;
using Rougamo.Context;
using Rougamo.Metadatas;

namespace BasicUsage.Attributes
{
    [Pointcut(AccessFlags.Instance | AccessFlags.Constructor)]
    public class CtorInitAttribute : MoAttribute
    {
        public const string FLAG = nameof(CtorInitAttribute);

        public override void OnEntry(MethodContext context)
        {
            ((SyncExecution)context.Target).InstanceProp1 = FLAG;
        }
    }
}
