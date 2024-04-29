using Rougamo.Context;
using Rougamo;

namespace Issues.Attributes
{
    internal class _63_Attribute<T> : MoAttribute
    {
        public override void OnEntry(MethodContext context)
        {
            if (context.Arguments.Length == 1 && context.Arguments[0] is T)
            {
                context.Arguments[0] = default(T);

                context.RewriteArguments = true;
            }
        }
    }
}
