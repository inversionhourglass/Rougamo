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
            if (context.RealReturnType == typeof(int))
            {
                context.ReplaceReturnValue(this, IntValue);
            }
            else if (context.RealReturnType == typeof(string))
            {
                context.ReplaceReturnValue(this, StringValue);
            }
            else if (context.RealReturnType == typeof(IEnumerable<int>))
            {
                context.ReplaceReturnValue(this, IteratorValue);
            }
            else if (context.RealReturnType == typeof(int?))
            {
                context.ReplaceReturnValue(this, IntValue);
            }
        }
    }
}
