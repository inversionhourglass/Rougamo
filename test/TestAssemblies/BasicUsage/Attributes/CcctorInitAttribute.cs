using Rougamo.Context;
using Rougamo;
using Rougamo.Metadatas;

namespace BasicUsage.Attributes
{
    [Pointcut(AccessFlags.Static | AccessFlags.Constructor)]
    public class CcctorInitAttribute : MoAttribute
    {
        public const string FLAG = nameof(CcctorInitAttribute);

        public override void OnEntry(MethodContext context)
        {
            if (context.Method.IsStatic)
            {
                SyncExecution.StaticProp2 = FLAG;
            }
            else
            {
                ((SyncExecution)context.Target).InstanceProp2 = FLAG;
            }
        }
    }
}
