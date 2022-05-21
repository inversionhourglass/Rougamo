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
            if (context.ReturnType == typeof(int))
            {
                context.ReplaceReturnValue(IntValue);
            }
            else if (context.ReturnType == typeof(string))
            {
                context.ReplaceReturnValue(StringValue);
            }
        }
    }
}
