using Rougamo;
using Rougamo.Context;

namespace ExUsage.Attributes
{
    public class SucceedReplaceIntAttribute : ExMoAttribute
    {
        protected override void ExOnSuccess(MethodContext context)
        {
            if (context.ExReturnType == typeof(int))
            {
                context.ReplaceReturnValue(this, 0);
            }
        }
    }
}
