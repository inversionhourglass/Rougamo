using Rougamo.Context;
using System.Collections.Generic;

namespace BasicUsage.Attributes
{
    public class ExceptionHandleAttribute : ContainerAttribute
    {
        public static int IntValue = nameof(OnException).Length;

        public static string StringValue = nameof(OnException);

        public static IEnumerable<int> IteratorValue = new int[] { 999, 357, 633 };

        public override void OnException(MethodContext context)
        {
            base.OnException(context);
            if(context.TaskReturnType == typeof(int))
            {
                context.HandledException(this, IntValue);
            }
            else if(context.TaskReturnType == typeof(string))
            {
                context.HandledException(this, StringValue);
            }
            else if (context.TaskReturnType == typeof(IEnumerable<int>))
            {
                context.HandledException(this, IteratorValue);
            }
        }
    }
}
