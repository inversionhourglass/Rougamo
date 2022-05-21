using Rougamo.Context;

namespace BasicUsage.Attributes
{
    public class OnEntryAttribute : ContainerAttribute
    {
        public override void OnEntry(MethodContext context)
        {
            base.OnEntry(context);
        }
    }
}
