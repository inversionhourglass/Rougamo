using Rougamo;
using Rougamo.Context;

namespace ExUsage.Attributes
{
    public class EntryReplaceStringAttribute : ExMoAttribute
    {
        protected override void ExOnEntry(MethodContext context)
        {
            if (context.ExReturnType == typeof(string))
            {
                context.ReplaceReturnValue(this, context.Method.Name);
            }
        }
    }
}
