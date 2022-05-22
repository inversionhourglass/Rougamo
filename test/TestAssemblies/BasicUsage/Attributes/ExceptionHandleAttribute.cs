using Rougamo.Context;

namespace BasicUsage.Attributes
{
    public class ExceptionHandleAttribute : ContainerAttribute
    {
        public static int IntValue = nameof(OnException).Length;

        public static string StringValue = nameof(OnException);

        public override void OnException(MethodContext context)
        {
            base.OnException(context);
            if(context.RealReturnType == typeof(int))
            {
                context.HandledException(this, IntValue);
            }
            else if(context.RealReturnType == typeof(string))
            {
                context.HandledException(this, StringValue);
            }
        }
    }
}
