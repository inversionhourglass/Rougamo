using Rougamo;
using Rougamo.Context;

namespace BasicUsage.Attributes
{
    internal class RetryAttribute : MoAttribute
    {
        public override void OnEntry(MethodContext context)
        {
            context.RetryCount = 3;
        }

        public override void OnException(MethodContext context)
        {
            context.RetryCount--;
        }

        public override void OnSuccess(MethodContext context)
        {
            context.RetryCount--;
        }
    }
}
