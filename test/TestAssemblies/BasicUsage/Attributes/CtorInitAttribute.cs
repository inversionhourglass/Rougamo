using Rougamo;
using Rougamo.Context;

namespace BasicUsage.Attributes
{
    public class CtorInitAttribute : MoAttribute
    {
        public const string FLAG = nameof(CtorInitAttribute);

        public override AccessFlags Flags => AccessFlags.Instance | AccessFlags.Constructor;

        public override void OnEntry(MethodContext context)
        {
            ((SyncExecution)context.Target).InstanceProp1 = FLAG;
        }
    }
}
