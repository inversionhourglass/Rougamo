using Rougamo.Context;
using System.Collections.Generic;

namespace BasicUsage.Attributes
{
    public class ReturnValueReplaceAttribute : ContainerAttribute
    {
        public static int IntValue = nameof(OnSuccess).Length;

        public static string StringValue = nameof(OnSuccess);

        public static IEnumerable<int> IteratorValue = new int[] { 135, 222, 790 };

        public override void OnSuccess(MethodContext context)
        {
            base.OnSuccess(context);
            if (context.TaskReturnType == typeof(int))
            {
                context.ReplaceReturnValue(this, IntValue);
            }
            else if (context.TaskReturnType == typeof(string))
            {
                context.ReplaceReturnValue(this, StringValue);
            }
            else if (context.TaskReturnType == typeof(IEnumerable<int>))
            {
                context.ReplaceReturnValue(this, IteratorValue);
            }
            else if (context.TaskReturnType == typeof(int?))
            {
                context.ReplaceReturnValue(this, IntValue);
            }
        }
    }
}
