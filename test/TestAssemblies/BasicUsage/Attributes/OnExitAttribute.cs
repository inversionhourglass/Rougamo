using Rougamo.Context;

namespace BasicUsage.Attributes
{
    public class OnExitAttribute : ContainerAttribute
    {
        public override void OnExit(MethodContext context)
        {
            base.OnExit(context);
        }
    }
}
