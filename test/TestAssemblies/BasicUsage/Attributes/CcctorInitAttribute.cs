using Rougamo.Context;
using Rougamo;

namespace BasicUsage.Attributes
{
    public class CcctorInitAttribute : MoAttribute
    {
        public const string FLAG = nameof(CcctorInitAttribute);

        public override AccessFlags Flags => AccessFlags.Static | AccessFlags.Constructor;

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
