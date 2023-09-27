using Rougamo;
using Rougamo.Context;

namespace BasicUsage.Attributes
{
    public class CctorInitAttribute : MoAttribute
    {
        public const string FLAG = nameof(CctorInitAttribute);

        public override AccessFlags Flags => AccessFlags.Static | AccessFlags.Constructor;

        public override void OnEntry(MethodContext context)
        {
            SyncExecution.StaticProp1 = FLAG;
        }
    }
}
