using Rougamo.Context;

namespace BasicUsage.Attributes
{
    public class OnSuccessAttribute : ContainerAttribute
    {
        public override void OnSuccess(MethodContext context)
        {
            base.OnSuccess(context);
        }
    }
}
