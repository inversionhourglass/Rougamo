using Rougamo;
using Rougamo.Context;

namespace Issues.Attributes
{
    public class _32_Attribute : ExMoAttribute
    {
        public const int IntValue = 1;

        protected override void ExOnSuccess(MethodContext context)
        {
            var resValue = context.ExReturnValue == null ? -1 : IntValue;
            context.ReplaceReturnValue(this, resValue);
        }
    }
}
