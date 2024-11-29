using Rougamo;
using Rougamo.Context;

namespace BasicUsage.Attributes
{
    public class ReplaceValueOnEntryAttribute : MoAttribute
    {
        public static string[] ArrayValue = new[] { nameof(ReplaceValueOnEntryAttribute), nameof(OnEntry) };

        public override void OnEntry(MethodContext context)
        {
            if (context.TaskReturnType == typeof(string[]))
            {
                context.ReplaceReturnValue(this, ArrayValue);
            }
            else if(context.TaskReturnType == typeof(long))
            {
                context.ReplaceReturnValue(this, null);
            }
            else if(context.TaskReturnType == typeof(long?))
            {
                context.ReplaceReturnValue(this, null);
            }
        }
    }
}
