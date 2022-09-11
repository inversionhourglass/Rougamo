using Rougamo;
using Rougamo.Context;
using System;

namespace ExUsage.Attributes
{
    public class ExceptionReplaceDoubleAttribute : ExMoAttribute
    {
        protected override void ExOnException(MethodContext context)
        {
            if (context.ExReturnType == typeof(double))
            {
                context.HandledException(this, Math.E);
            }
        }
    }
}
