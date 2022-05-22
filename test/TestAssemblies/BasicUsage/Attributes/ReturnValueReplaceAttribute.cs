using Rougamo.Context;

namespace BasicUsage.Attributes
{
    public class ReturnValueReplaceAttribute : ContainerAttribute
    {
        public static int IntValue = nameof(OnSuccess).Length;

        public static string StringValue = nameof(OnSuccess);

        public override void OnSuccess(MethodContext context)
        {
            base.OnSuccess(context);
            if (context.RealReturnType == typeof(int))
            {
                context.ReplaceReturnValue(this, IntValue);
            }
            else if (context.RealReturnType == typeof(string))
            {
                context.ReplaceReturnValue(this, StringValue);
            }
        }
    }
}
