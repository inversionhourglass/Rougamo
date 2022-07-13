using Rougamo.Context;

namespace BasicUsage.Attributes
{
    public class ReplaceValueOnEntryAttribute : ContainerAttribute
    {
        public static string[] ArrayValue = new[] { nameof(ReplaceValueOnEntryAttribute), nameof(OnEntry) };

        public override void OnEntry(MethodContext context)
        {
            base.OnEntry(context);
            if (context.RealReturnType == typeof(string[]))
            {
                context.ReplaceReturnValue(this, ArrayValue);
            }
            else if(context.RealReturnType == typeof(long))
            {
                context.ReplaceReturnValue(this, null);
            }
            else if(context.RealReturnType == typeof(long?))
            {
                context.ReplaceReturnValue(this, null);
            }
        }
    }
}
